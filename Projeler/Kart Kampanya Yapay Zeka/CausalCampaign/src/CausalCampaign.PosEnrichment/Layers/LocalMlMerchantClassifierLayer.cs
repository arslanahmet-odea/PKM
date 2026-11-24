using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Enums;
using CausalCampaign.PosEnrichment.Models;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;

namespace CausalCampaign.PosEnrichment.Layers;

/// <summary>
/// Layer 3: Classifies long-tail and noisy POS merchant texts unmatched by the first two layers into
/// 9 standard sectors using ML.NET (Character + Word N-Gram FeaturizeText and LightGbmMulticlass).
/// </summary>
public sealed class LocalMlMerchantClassifierLayer : IPosClassificationLayerHandler
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _trainedModel;
    private readonly PredictionEngine<MerchantTextInput, MerchantSectorPrediction> _predictionEngine;
    private readonly PaymentFacilitatorRegexLayer _pfParser;
    private readonly object _predictLock = new();

    public LocalMlMerchantClassifierLayer(PaymentFacilitatorRegexLayer pfParser)
    {
        _pfParser = pfParser;
        _mlContext = new MLContext(seed: 42);
        _trainedModel = BuildAndTrainCorpusModel(_mlContext);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<MerchantTextInput, MerchantSectorPrediction>(_trainedModel);
    }

    /// <summary>
    /// Classifies a long-tail POS merchant text and MCC code using the local ML.NET model.
    /// </summary>
    public LayerClassificationMatch TryClassify(string normalizedMerchantText, string rawMcc)
    {
        var (provider, cleanedSubText) = _pfParser.ParsePaymentFacilitator(normalizedMerchantText);
        string textForClassification = string.IsNullOrWhiteSpace(cleanedSubText)
            ? normalizedMerchantText
            : cleanedSubText;

        bool isExcludedMcc = AntiGamingConstants.IsExcludedMcc(rawMcc);

        MerchantSectorPrediction prediction;
        lock (_predictLock)
        {
            prediction = _predictionEngine.Predict(new MerchantTextInput
            {
                MerchantText = textForClassification,
                MccCode = rawMcc ?? "0000"
            });
        }

        string resolvedSector = isExcludedMcc
            ? "Cash-Like / Excluded"
            : string.IsNullOrWhiteSpace(prediction.PredictedSector)
                ? "Grocery & Supermarket"
                : prediction.PredictedSector;

        decimal confidence = CalculateSoftmaxConfidence(prediction.Scores);
        bool isExcluded = isExcludedMcc || resolvedSector == "Cash-Like / Excluded";

        string cleanedBrand = DeterministicBrandDictionaryLayer.ExtractFallbackBrandTitle(textForClassification);

        return new LayerClassificationMatch(
            CleanedBrandName: cleanedBrand,
            ResolvedSector: resolvedSector,
            SubMerchantProvider: provider,
            Layer: PosClassificationLayer.MachineLearning,
            ConfidenceScore: confidence,
            IsCashLikeOrExcludedMcc: isExcluded);
    }

    private static decimal CalculateSoftmaxConfidence(float[]? scores)
    {
        if (scores is null || scores.Length == 0)
        {
            return 0.8500m;
        }

        float maxScore = scores.Max();
        double sumExp = scores.Sum(s => Math.Exp(s - maxScore));
        if (sumExp <= 0)
        {
            return 0.8500m;
        }

        double maxProb = 1.0 / sumExp;
        return Math.Round((decimal)Math.Clamp(maxProb, 0.50, 0.99), 4);
    }

    private static ITransformer BuildAndTrainCorpusModel(MLContext mlContext)
    {
        var corpus = BuildPosTrainingCorpus();
        IDataView trainingData = mlContext.Data.LoadFromEnumerable(corpus);

        var textOptions = new TextFeaturizingEstimator.Options
        {
            CaseMode = TextNormalizingEstimator.CaseMode.Upper,
            WordFeatureExtractor = new WordBagEstimator.Options
            {
                NgramLength = 2,
                UseAllLengths = true
            },
            CharFeatureExtractor = new WordBagEstimator.Options
            {
                NgramLength = 3,
                UseAllLengths = false
            }
        };

        var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(MerchantTextInput.SectorLabel))
            .Append(mlContext.Transforms.Text.FeaturizeText("MerchantTextFeatures", textOptions, nameof(MerchantTextInput.MerchantText)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("MccFeatures", nameof(MerchantTextInput.MccCode)))
            .Append(mlContext.Transforms.Concatenate("Features", "MerchantTextFeatures", "MccFeatures"))
            .Append(mlContext.MulticlassClassification.Trainers.LightGbm(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 15,
                minimumExampleCountPerLeaf: 1,
                learningRate: 0.15,
                numberOfIterations: 60))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        return pipeline.Fit(trainingData);
    }

    private static List<MerchantTextInput> BuildPosTrainingCorpus()
    {
        return
        [
            // 1. Grocery & Supermarket
            new() { MerchantText = "BEREKET UNLU MAMULLERI VE GIDA SAN", MccCode = "5411", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "ANADOLU ET GALERISI KASAP SARKUTERI", MccCode = "5422", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "YESILKOY MANAV SEBZE MEYVE GIDA", MccCode = "5411", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "KARADENIZ FIRIN EKMEK UNLU MAMULLER", MccCode = "5462", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "DOGAL SARKUTERI PEYNIR ZEYTIN GIDA", MccCode = "5499", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "MAHALLE HIPERMARKET SUPERMARKET GIDA", MccCode = "5411", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "GIDA PAZARLAMA KURUYEMIS BAHARAT", MccCode = "5499", SectorLabel = "Grocery & Supermarket" },
            new() { MerchantText = "ORGANIK TARIM GIDA MARKET PERAKENDE", MccCode = "5411", SectorLabel = "Grocery & Supermarket" },

            // 2. Fuel
            new() { MerchantText = "AKDENIZ AKARYAKIT ISTASYONU PETROL", MccCode = "5541", SectorLabel = "Fuel" },
            new() { MerchantText = "MARMARA PETROLCULUK LPG OTOGAZ", MccCode = "5542", SectorLabel = "Fuel" },
            new() { MerchantText = "EGE ENERJI AKARYAKIT SERVIS ISTASYONU", MccCode = "5541", SectorLabel = "Fuel" },
            new() { MerchantText = "OTOYOL DINLENME PETROL AKARYAKIT", MccCode = "5541", SectorLabel = "Fuel" },
            new() { MerchantText = "SARJ ISTASYONU ELEKTRIKLI ARAC ENERJI", MccCode = "5541", SectorLabel = "Fuel" },
            new() { MerchantText = "KILICLAR PETROL URU AKARYAKIT", MccCode = "5541", SectorLabel = "Fuel" },

            // 3. E-Commerce & Marketplace
            new() { MerchantText = "DIJITAL PAZARYERI E TICARET PLATFORMU", MccCode = "5399", SectorLabel = "E-Commerce & Marketplace" },
            new() { MerchantText = "ONLINE ALISVERIS MAGAZASI E-TICARET", MccCode = "5399", SectorLabel = "E-Commerce & Marketplace" },
            new() { MerchantText = "SANAL MAGAZA INTERNET SATIS PAZARYERI", MccCode = "5969", SectorLabel = "E-Commerce & Marketplace" },
            new() { MerchantText = "GLOBAL E-COMMERCE MARKETPLACE", MccCode = "5399", SectorLabel = "E-Commerce & Marketplace" },
            new() { MerchantText = "HIZLI TESLIMAT ONLINE SIPARIS", MccCode = "7399", SectorLabel = "E-Commerce & Marketplace" },

            // 4. Apparel & Fashion
            new() { MerchantText = "MODA BUTIK KONFEKSIYON TEKSTIL GIYIM", MccCode = "5651", SectorLabel = "Apparel & Fashion" },
            new() { MerchantText = "DERI AYAKKABI CANTA AKSESUAR MAGAZASI", MccCode = "5661", SectorLabel = "Apparel & Fashion" },
            new() { MerchantText = "ISTANBUL TEKSTIL HAZIR GIYIM MAGAZACILIK", MccCode = "5691", SectorLabel = "Apparel & Fashion" },
            new() { MerchantText = "SPOR GIYIM OUTDOOR AYAKKABI TEKSTIL", MccCode = "5655", SectorLabel = "Apparel & Fashion" },
            new() { MerchantText = "COCUK BEBEK GIYIM KONFEKSIYON", MccCode = "5641", SectorLabel = "Apparel & Fashion" },
            new() { MerchantText = "TRIKO GOMLEK TAKIM ELBISE GIYIM", MccCode = "5611", SectorLabel = "Apparel & Fashion" },

            // 5. Electronics
            new() { MerchantText = "GUNES BILGISAYAR YAZILIM DONANIM ELEKTRONIK", MccCode = "5732", SectorLabel = "Electronics" },
            new() { MerchantText = "CEP TELEFONU MOBIL ILETISIM TEKNOLOJI", MccCode = "4812", SectorLabel = "Electronics" },
            new() { MerchantText = "BEYAZ ESYA ELEKTRIKLI EV ALETLERI", MccCode = "5722", SectorLabel = "Electronics" },
            new() { MerchantText = "BILISIM TEKNOLOJIMARKET ELEKTRONIK", MccCode = "5732", SectorLabel = "Electronics" },
            new() { MerchantText = "SES GORUNTU SISTEMLERI KAMERA ELEKTRONIK", MccCode = "5732", SectorLabel = "Electronics" },

            // 6. Restaurant & Cafe
            new() { MerchantText = "LEZZET DURAGI KEBAP LAHMACUN RESTORAN", MccCode = "5812", SectorLabel = "Restaurant & Cafe" },
            new() { MerchantText = "MODA ROASTERY COFFEE KAFE PASTANESI", MccCode = "5814", SectorLabel = "Restaurant & Cafe" },
            new() { MerchantText = "BOGAZICI BALIK RESTAURANT LOKANTA", MccCode = "5812", SectorLabel = "Restaurant & Cafe" },
            new() { MerchantText = "PIZZA BURGER FAST FOOD RESTORAN", MccCode = "5814", SectorLabel = "Restaurant & Cafe" },
            new() { MerchantText = "TATLICI BAKLAVA DONDURMA KAFE", MccCode = "5812", SectorLabel = "Restaurant & Cafe" },
            new() { MerchantText = "STEAKHOUSE IZGARA ET LOKANTASI", MccCode = "5812", SectorLabel = "Restaurant & Cafe" },

            // 7. Travel & Hospitality
            new() { MerchantText = "GRAND RESORT SPA HOTEL TURIZM KONAKLAMA", MccCode = "7011", SectorLabel = "Travel & Hospitality" },
            new() { MerchantText = "HAVAYOLLARI UCAK BILETI SEYAHAT ACENTESI", MccCode = "4511", SectorLabel = "Travel & Hospitality" },
            new() { MerchantText = "OTO KIRALAMA RENT A CAR TURIZM SEYAHAT", MccCode = "7512", SectorLabel = "Travel & Hospitality" },
            new() { MerchantText = "BUTIK OTEL PANSIYON TATIL KOYU KONAKLAMA", MccCode = "7011", SectorLabel = "Travel & Hospitality" },
            new() { MerchantText = "TURIZM TASIMACILIK OTOBUS BILET SEYAHAT", MccCode = "4131", SectorLabel = "Travel & Hospitality" },

            // 8. Health & Cosmetics
            new() { MerchantText = "SIFA ECZANESI MEDIKAL SAGLIK URUNLERI", MccCode = "5912", SectorLabel = "Health & Cosmetics" },
            new() { MerchantText = "OZEL POLIKLINIK DIS HASTANESI SAGLIK", MccCode = "8099", SectorLabel = "Health & Cosmetics" },
            new() { MerchantText = "GUZELLIK KOZMETIK PARFUMERI BAKIM", MccCode = "5977", SectorLabel = "Health & Cosmetics" },
            new() { MerchantText = "OPTIK GOZLUK LENS MEDIKAL SAGLIK", MccCode = "8043", SectorLabel = "Health & Cosmetics" },
            new() { MerchantText = "ECZANE DERMOKOZMETIK VITAMIN SAGLIK", MccCode = "5912", SectorLabel = "Health & Cosmetics" },

            // 9. Cash-Like / Excluded
            new() { MerchantText = "KAPALICARSI SARRAFIYE ALTIN KUYUMCULUK", MccCode = "6051", SectorLabel = "Cash-Like / Excluded" },
            new() { MerchantText = "DOVIZ BUROSU KIYMETLI MADENLER", MccCode = "6051", SectorLabel = "Cash-Like / Excluded" },
            new() { MerchantText = "SANS OYUNLARI BAHIS INTERAKTIF", MccCode = "7995", SectorLabel = "Cash-Like / Excluded" },
            new() { MerchantText = "KRIPTO VARLIK DIJITAL CUZDAN TRANSFER", MccCode = "6051", SectorLabel = "Cash-Like / Excluded" },
            new() { MerchantText = "PARA TRANSFERI HAVALE FINANSAL", MccCode = "4829", SectorLabel = "Cash-Like / Excluded" }
        ];
    }
}
