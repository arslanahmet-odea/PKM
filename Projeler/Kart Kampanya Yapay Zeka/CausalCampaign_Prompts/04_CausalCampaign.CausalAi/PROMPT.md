# PROJE 4 PROMPTU: `CausalCampaign.CausalAi` (R-Learner Nedensel Uplift, Bandit, XAI, HDBSCAN & CUPED Motoru)

*(3. Projeyi inceleyip onayladıktan sonra, 4. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0, Nedensel Makine Öğrenmesi (Causal ML / ML.NET), İstatistiksel Modelleme (`MathNet.Numerics`) ve Microsoft SQL Server konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **4. Aşamasına (`src/CausalCampaign.CausalAi` — Enflasyon-Düzeltmeli Feature Store, Robinson R-Learner Nedensel Uplift, Contextual Bandit, XAI, HDBSCAN, PSI Drift ve CUPED Deney Motoru)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını koru (`CausalCampaign.CausalAi`, `CausalCampaign.Domain` içindeki `ICausalUpliftEngine`, `IXaiExplanationService`, `ICupedExperimentEvaluator`, `IDriftAndClusteringService` arayüzlerini uygular).
   - Veritabanı erişimi yalnızca `ICausalCampaignDbContext` (EF Core 10.0) üzerinden yapılacaktır.
   - Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
   - Sıfıra bölünme (propensity clipping $\hat{e}(X) \in [0.05, 0.95]$), sıfır varyans ve küçük örneklem sınır durumlarını matematiksel olarak güvenli şekilde ele al.

3. **Hata Yönetimi ve Güvenlik:**
   - Tüm model tahmin ve değerlendirme çıktılarında `Result<T>` desenini kullan.
   - Model hiperparametrelerini (`ShrinkagePriorStrength`, `PropensityClipMin`, `PropensityClipMax`, `PsiAlertThreshold`) kod içine gömme; `IOptions<CausalAiOptions>` ile yapılandır.

4. **Test ve Dokümantasyon:**
   - Testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - Tüm matematiksel formüllerin (Robinson R-Learner, Empirik Bayesyen Küçültme, Thompson Sampling, PSI, HDBSCAN, CUPED) üzerine detaylı **XML Documentation (`/// <summary>`)** açıklamaları yaz.

---

### BU AŞAMADA (`CausalCampaign.CausalAi`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.CausalAi` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve `CausalCampaign.Domain` referansını bağla.
   - Paketler: `Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender`, `MathNet.Numerics`.
2. **Enflasyon-Düzeltmeli Feature Store (`FeatureStore/InflationAdjustedFeatureStore.cs`):**
   - EF Core üzerinden `vw_CustomerCausalFeatures` ve `INFLATION_INDEX_MONTHLY` verilerini okuyarak nominal TL yerine reel harcama endeksi (`RealBaselineSpendIndex`), portföy harcama yüzdelik dilimi (`PortfolioRealSpendQuantile`), sektör çeşitliliği, boş limit oranı (`AvailableLimitRatio`) ve MIS kârlılık vektörünü üreten servis.
3. **İki Aşamalı Robinson R-Learner ve Empirik Bayesyen Küçültme Motoru (`Uplift/RobinsonRLearnerUpliftEngine.cs`):**
   - **1. Aşama (Organik Harcama $\hat{\mu}_0(X)$ ve Müdahale Olasılığı $\hat{e}(X)$):**
     - `MLContext.Regression.Trainers.LightGbm` ile müşterinin kampanya olmasa dahi yapacağı **Beklenen Organik Harcamayı** ($\hat{\mu}_0(X)$) tahmin et.
     - Kampanya alma olasılığını $\hat{e}(X) \in [0.05, 0.95]$ aralığında kırparak tahmin et.
   - **2. Aşama (Sapma Artıkları Üzerinden Bireysel Kampanya Etkisi $\hat{\tau}(X)$):**
     - Harcama artığı $Y_{res} = Y - \hat{\mu}_0(X)$ ve müdahale artığı $W_{res} = W - \hat{e}(X)$ üzerinden sözde-hedef $\tilde{\tau}_i = Y_{res,i} / W_{res,i}$ ve ağırlık $w_i = W_{res,i}^2$ ile ikinci `LightGbm` modelini eğit.
     - **Empirik Bayesyen Küçültme (Shrinkage):** `MathNet.Numerics` kullanarak düşük örneklem belirsizliğini bastırmak üzere bireysel tahmini segment ortalamasına doğru küçült: $\hat{\tau}_{Bayes,i} = \lambda_i \hat{\tau}_i + (1 - \lambda_i) \bar{\tau}_{segment}$.
     - **"Free-Rider" Tespiti (`KPI-2`):** Organik harcaması yüksek ($\hat{\mu}_0(X)$ yüksek) ancak marjinal kampanya etkisi ($\hat{\tau}_{Bayes,i}$) düşük olan müşterileri `FreeRider_SureThing` olarak işaretle.
   - **İnaktif (Uyuyan) Kart Uyandırma Modeli (`Uplift/DormantCardReactivationModel.cs`):**
     - `IsDormant == true` olan müşteriler için 60 günlük kalıcı aktifleşme olasılığını (`ReactivationProbability`, `KPI-4: => %6,5`) ve optimal ilk harcama eşiğini hesapla.
4. **Sektörel Öneri (`Recommendation/MatrixFactorizationSectorRecommender.cs`) ve Contextual Bandit (`Bandit/ThompsonSamplingBanditService.cs`):**
   - `Microsoft.ML.Recommender` (`MatrixFactorizationTrainer`) ile müşterinin henüz harcama yapmadığı ancak en yüksek geçiş potansiyeli taşıdığı hedef sektörü tahmin et.
   - `MathNet.Numerics.Distributions.Beta` kullanarak hedef/ödül paketlerinde keşif-kullanım (exploration-exploitation) yapan ve müşterinin Puan vs. Nakit İade seçiminden (Discrete Choice) öğrenen servisi kodla.
5. **Türkçe XAI Gerekçe Motoru (`Explainability/XaiTurkishExplanationService.cs`):**
   - ML.NET `CalculateFeatureContributions` ve öznitelik etki ağırlıklarını kullanarak her müşteri için en etkili 3 faktörü çıkaran ve Türkçe karar gerekçesi (`XaiReasonText` + `XaiTopFeaturesJson`) üreten servis (`KPI-8: %100 Gerekçe Üretimi`).
6. **HDBSCAN Kümeleme, PSI Drift ve CUPED Deney Doğrulayıcı (`Analytics/`):**
   - `BehavioralHdbscanClusterer.cs`: Yoğunluk tabanlı davranışsal mikro-küme keşfi ve analist fırsat özeti üretimi.
   - `InflationPsiDriftMonitor.cs`: Eğitim ve güncel dönem dağılımları arasında `PSI = Sum((Actual% - Expected%) * ln(Actual% / Expected%))` hesaplayan ve `PSI < 0.15` (`KPI-7`) kararlılığını denetleyen servis.
   - `CupedExperimentEvaluator.cs`: Deney öncesi harcamayı ($X_{pre}$) ortak değişken olarak kullanan **CUPED** ($\theta = \text{Cov}(Y_{post}, X_{pre}) / \text{Var}(X_{pre})$, $\hat{Y}_{CUPED} = Y_{post} - \theta (X_{pre} - \bar{X}_{pre})$) yöntemiyle `(İlave Harcama * Marj - Ödül)` net ekonomik katkısını ve `Incremental ROI => 1.35x` (`KPI-3`) artışını doğrulayan servis.
7. **Birim Testler (`tests/CausalCampaign.UnitTests/CausalAiTests/`):**
   - `MSTest` ve `Moq` kullanarak R-Learner Free-Rider ayrımını (`KPI-2: <= %25`), Uyuyan Kart uyandırma dönüşümünü (`KPI-4: => %6,5`), CUPED varyans azaltma ve `ROI => 1.35x` hesabını (`KPI-3`), `PSI < 0.15` kontrolünü (`KPI-7`) ve Türkçe XAI gerekçe üretimini (`KPI-8`) doğrulayan kapsamlı birim testleri.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.CausalAi` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender`, `MathNet.Numerics`, `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** 2 aşamalı Robinson R-Learner + Empirik Bayesyen Küçültme matematiği, enflasyon deflatör yaklaşımı ve CUPED varyans azaltma tasarımı.

Sonunda **"4. Aşama (`CausalCampaign.CausalAi`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile doğrulayıp incelemem için dur.
```
