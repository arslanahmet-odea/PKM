# ODEA-028: Claude Code ve GitHub Copilot İçin Tam Kapsamlı C# (.NET) & SQL Server Geliştirme Prompt Rehberi

Bu doküman, **ODEA-028 (Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru)** projesini **%100 C# (.NET 8.0/9.0) ve Microsoft SQL Server** kullanarak **Claude Code** veya **GitHub Copilot (Agent / Edits Modu)** ile uçtan uca, eksiksiz ve derlenebilir şekilde yazdırmanız için hazırlanmıştır.

---

## 🛠️ Nasıl Kullanmalısınız?

1. **Tek Seferde (Otonom) Yazdırmak İçin (Claude Code Önerilen Yöntem):**
   * Proje klasörünüzde `CLAUDE.md` (veya GitHub Copilot için `.github/copilot-instructions.md`) dosyası oluşturup **BÖLÜM 1'deki "Ana Mimari ve Sistem Kuralları Promptu"**nu içine yapıştırın.
   * Ardından Claude Code'a veya Copilot Agent'a **BÖLÜM 2'deki "Uçtan Uca Tek Seferlik Mega-Prompt"**u verin.
2. **Adım Adım (Sprint Bazlı Kontrollü) Yazdırmak İçin (GitHub Copilot & Claude Code En Güvenli Yöntem):**
   * Büyük dil modellerinin hiçbir detayı atlamaması (`// TODO` bırakmaması) için **BÖLÜM 3'teki 6 Adımlık Sıralı Promptları (Prompt 1 -> Prompt 6)** sırayla verip her adım sonunda `dotnet build` ve `dotnet test` çalıştırmasını isteyin.

---

## BÖLÜM 1: `CLAUDE.md` ve `.github/copilot-instructions.md` İçin Ana Sistem Kuralları

*(Aşağıdaki bloğu kopyalayıp proje kök dizinindeki `CLAUDE.md` ve `.github/copilot-instructions.md` dosyalarına yapıştırın.)*

```markdown
# ODEA-028: Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru

## 1. Proje Kimliği ve Temel Kurallar
- **Proje Kodu ve Adı:** ODEA-028 - Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru Geliştirilmesi
- **Teknoloji Yığını (%100 C# / .NET & Microsoft SQL Server):**
  - **Hedef Framework:** `.NET 8.0` (veya `.NET 9.0`)
  - **Veritabanı:** Microsoft SQL Server (`appsettings.json` içindeki `ConnectionStrings:DefaultConnection` üzerinden bağlanılacak)
  - **Veri Erişimi:** `Microsoft.Data.SqlClient`, `Dapper` (< 500 ms gerçek zamanlı provizyon ve sayaç işlemleri için), `SqlBulkCopy` (toplu veri yükleme için)
  - **Yapay Zekâ / Makine Öğrenmesi:** `Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender` (`MatrixFactorization`), `Microsoft.ML.OnnxRuntime`
  - **Matematiksel Optimizasyon (MILP):** `Google.OrTools` (.NET resmi paketi - `SCIP` veya `CBC_MIXED_INTEGER_PROGRAMMING` çözücüsü)
  - **İstatistik, Bayesyen Küçültme, Bandit & CUPED:** `MathNet.Numerics`
  - **Backend API & Arayüz (UI):** `ASP.NET Core Web API` + `Blazor Web App (Interactive Server)` (Node.js/npm bağımlılığı olmadan %100 C# ile çalışan 3'ü 1 arada arayüz)
  - **Test Altyapısı:** `xUnit`, `FluentAssertions`

## 2. Kesin Kodlama Kısıtları (Guardrails)
1. **Sıfır Placeholder Kuralı:** Hiçbir sınıfta veya metotta `// TODO`, `throw new NotImplementedException()`, boş gövde veya sahte sabit dönüş (`return 0;`) bırakma. Tüm algoritmaları, SQL sorgularını ve matematiksel formülleri gerçek çalışan C# koduyla yaz.
2. **Dil ve Terminoloji:** Kullanıcı arayüzü (UI) metinleri, SHAP/XAI müşteri gerekçeleri, log mesajları ve XML yorumları Türkçe olmalıdır.
3. **Sembol Kuralı:** Rapor, arayüz ve dokümantasyon çıktılarında büyük-eşit ve küçük-eşit için yalnızca `=>` ve `<=` kullan.
4. **Derleme ve Test Doğrulaması:** Yazdığın her modülden sonra `dotnet build` ve `dotnet test` komutlarını çalıştırarak sıfır derleme hatası ve %100 geçen testlerle doğrula.

## 3. Sağlanması Zorunlu 8 Başarı Ölçütü (KPI)
- **KPI-1:** POS İşlem Metni (`DE-43` & `DE-18 MCC`) Marka/Sektör Sınıflandırma Doğruluğu (Macro F1-Score) `=> %92,0`
- **KPI-2:** "Free-Rider" (Organik Harcamaya Ödenen Gereksiz Ödül) Oranı `<= %25,0`
- **KPI-3:** Birim Ödül Maliyeti Başına Net İlave Ekonomik Katkı (`Incremental ROI`) `=> 1,35x` (`=> +%35` artış, CUPED ile doğrulanmış)
- **KPI-4:** İnaktif (Uyuyan) Kredi Kartı Uyandırma (`Reactivation`) Dönüşüm Oranı `=> %6,5`
- **KPI-5:** Gerçek Zamanlı Provizyon Sayaç Güncelleme Gecikmesi (`End-to-End Latency`) `< 500 ms (P95)`
- **KPI-6:** Portföy Geneli Kısıtlı Bütçe Optimizasyon Çözüm Süresi `< 15 Dakika`
- **KPI-7:** Enflasyonist Veri Kayması Kararlılığı (`Aylık PSI < 0,15` ve `Qini Katsayısı Kaybı < %5`)
- **KPI-8:** Açıklanabilirlik (XAI) ve Kampanya Çakışma / Suistimal Hata Oranı (`%0 Kural İhlali / %100 Gerekçe Üretimi`)
```

---

## BÖLÜM 2: Uçtan Uca Tek Seferlik Mega-Prompt (Claude Code / Copilot Agent İçin)

*(Tüm projeyi tek bir kapsamlı talimatla başlatmak isterseniz aşağıdaki bloğu doğrudan Claude Code'a veya GitHub Copilot Agent'a yapıştırın. `CONNECTION_STRING_BURAYA` kısmına kendi SQL Server bağlantı cümlenizi yazın.)*

```text
Sen kıdemli bir .NET Sistem Mimarı, Finansal Veri Bilimci (ML.NET) ve Yöneylem Araştırması (Google OR-Tools) uzmanısın.
ODEA-028 kodlu "Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru" projesini %100 C# (.NET 8.0) ve Microsoft SQL Server kullanarak uçtan uca, hiçbir yerde "// TODO" veya "NotImplementedException" bırakmadan geliştireceksin.

SQL Server Bağlantı Cümlem (Connection String):
"CONNECTION_STRING_BURAYA"
(Not: Veritabanı yoksa otomatik oluşturan DDL/Initializer kodunu yaz.)

Lütfen aşağıdaki Solution (.sln) yapısını oluştur ve 6 modülün tamamını sırasıyla kodla, derle (`dotnet build`) ve test et (`dotnet test`):

1. SOLUTION YAPISI: `Odea028.CausalCampaignEngine.sln`
- `src/Odea028.Core`: Tüm domain varlıkları (Entities), DTO'lar, Enum'lar, iş kuralı sabitleri ve servis arayüzleri (`Interfaces`).
- `src/Odea028.Infrastructure`: SQL Server bağlantı fabrikası (`SqlConnectionFactory`), Dapper repository'leri, tüm tabloları ve görünümleri (View/Index) otomatik kuran `DatabaseInitializer` ve gerçekçi banka verisi üreten `SyntheticBankDataSeeder` (`SqlBulkCopy` ile).
- `src/Odea028.PosEnrichment`: 3 Katmanlı POS İşlem Zenginleştirme Motoru:
  - Katman 1: Ulusal zincir sözlüğü (`FrozenDictionary` + SQL Server `MERCHANT_MASTER_CACHE` tablosundan O(1) okuma).
  - Katman 2: C# `[GeneratedRegex]` ile ödeme kuruluşu (`IYZICO*`, `PARAM*`, `PAYTR*`, `SIPAY*`, `MOKA*`) ön eklerini temizleyip alt-işyeri ve markayı çıkaran ayrıştırıcı.
  - Katman 3: Uzun kuyruk (long-tail) ve bozuk POS metinlerini `DE-43` ve `DE-18 MCC` üzerinden gerçek sektöre sınıflandıran `Microsoft.ML` (`FeaturizeText` + `LightGbmMulticlass`) modeli (Macro F1-Score => %92,0).
- `src/Odea028.CausalAi`:
  - Enflasyon-Düzeltmeli Feature Store: SQL Server üzerinde sektörel aylık enflasyon endeksiyle (`INFLATION_INDEX_MONTHLY`) reel harcamayı ve portföy kuantilini (`PERCENT_RANK` / `NTILE`) hesaplayan servis.
  - İki Aşamalı Robinson R-Learner Uplift Motoru (`Microsoft.ML.LightGbm` + `MathNet.Numerics`):
    - 1. Aşama: Kampanyasız Beklenen Organik Harcama (`ExpectedOrganicSpend`) ve müdahale olasılığı (`PropensityScore`) tahmini.
    - 2. Aşama: Sapma artıkları (`Gerçekleşen Harcama - Beklenen Organik Harcama`) üzerinden sözde-hedef (pseudo-outcome) ağırlıklı ikinci `LightGbm` regresyon modeli ve Empirik Bayesyen Küçültme (Shrinkage) ile Bireysel Kampanya Etkisi (`IndividualTreatmentEffect` / Uplift) ve İnaktif Kart Uyandırma (`ReactivationProbability`) hesabı.
  - Sektörel Öneri Motoru: `Microsoft.ML.Recommender` (`MatrixFactorizationTrainer`) ile müşterinin henüz harcama yapmadığı ancak yüksek potansiyel taşıdığı sektörlerin tahmini.
  - Contextual Bandit: `MathNet.Numerics.Distributions.Beta` ve `Normal` ile Thompson Sampling keşif-kullanım (exploration-exploitation) modülü ve Puan vs. Nakit İade (Cashback) tercih öğrenme (Discrete Choice) güncelleyicisi.
  - Açıklanabilir Yapay Zekâ (XAI): ML.NET `CalculateFeatureContributions` kullanarak her müşteri kararı için en etkili 3 özniteliği çıkaran ve Odea Mobil / Çağrı Merkezi için Türkçe gerekçe cümlesi üreten motor.
  - Davranışsal Kümeleme ve Deney Ölçümü: C# HDBSCAN/Yoğunluk tabanlı mikro-küme keşif algoritması, PSI (Population Stability Index) enflasyon kayması hesaplayıcı ve Stratified RCT + CUPED (`Deney Öncesi Veriyle Varyans Azaltma`) net ekonomik katkı (`İlave Harcama * Marj - Ödül`) doğrulayıcı.
- `src/Odea028.Optimization`:
  - `Google.OrTools.LinearSolver` (`SCIP` / `CBC`) tabanlı Karma Tamsayılı Doğrusal Programlama (MILP) çözücüsü:
    - Amaç Fonksiyonu: Toplam Net İlave Ekonomik Katkıyı (`(IncrementalSpend * MarginRate) - RewardCost`) maksimize et.
    - Kısıt 1: Her müşteriye en fazla 1 kampanya paketi atanabilir (veya kârsızsa/Free-Rider ise teklif atanmaz).
    - Kısıt 2: Toplam kampanya ödül bütçesi (`TotalBudget`) aşılamaz.
    - Kısıt 3: Atanan harcama hedefi, müşterinin kullanılabilir boş kredi kartı limitinin (`AvailableCreditLimit * 0.80`) üzerinde olamaz.
    - Kısıt 4: Müşterinin banka geneli MIS kârlılık skoru (`MisProfitabilityScore`) ve Ayrıcalıklar Dünyası segment kurallarıyla çelişemez.
    - Kısıt 5 (Algoritmik Adillik / Monotonluk Kısıtı): Aynı segmentte daha yüksek harcama hedefi (`TargetAmount`) atanan bir müşterinin mutlak ödül tutarı (`RewardAmount`), daha düşük hedef atanan müşteriden düşük olamaz (`Hedef_i > Hedef_j => Ödül_i => Ödül_j`).
  - Operasyonel yönetilebilirlik için bireysel hedefleri 20 standart Mikro-Kovaya (`MicroBucketId: 1..20`) yuvarlayan ve SQL Server `CUSTOMER_OFFER_ASSIGNMENT` ile `OFFER_PROGRESS_TRACKER` tablolarına yazan servis.
- `src/Odea028.Web` (`ASP.NET Core 8.0 + Blazor Interactive Server`):
  - Gerçek Zamanlı ISO 8583 Provizyon & Sayaç API'si (`POST /api/v1/transactions/authorize` ve `POST /api/v1/transactions/reversal`):
    - Anti-Gaming Kural 1: Nakit benzeri / yasaklı MCC (`6011`, `6012`, `6051`, `7995`, `4829` vb.) filtreleme.
    - Anti-Gaming Kural 2: Aynı üye işyerinden 30 dakika içinde yapılan bölünmüş işlemleri (split-transaction) tek işlem sayma veya engelleme.
    - Anti-Gaming Kural 3: İptal/İade (`Reversal / Refund`) geldiğinde sayacı düşürme ve kazanılmış ödülü geri alma (`Clawback` / `REWARD_FULFILLMENT_QUEUE` güncellemesi).
    - Dapper ile `< 500 ms` altında `OFFER_PROGRESS_TRACKER` güncellemesi.
  - 3'ü 1 Arada Blazor Web Arayüzü:
    - Sekme 1 (Odea Mobil Müşteri Simülatörü): Seçilen müşterinin Kişiye Özel Teklif Kartı, XAI Türkçe gerekçesi, 3 eşdeğer tekliften birini seçme (Tercih Öğrenme) ekranı, canlı "Simüle POS İşlemi Geçir / İade Geçir" butonu ve milisaniyeler içinde dolan İlerleme Çubuğu (`Progress Bar`).
    - Sekme 2 (Back-Office Yönetim, Simülasyon & Kill-Switch Paneli): R-Learner + MILP optimizasyonunu tetikleme, bütçe tüketim hızı (Burn-Rate), CUPED Net ROI karşılaştırması, PSI Drift göstergesi, HDBSCAN analist küme özeti ve acil durdurma (`Kill-Switch`) butonu.
    - Sekme 3 (Çağrı Merkezi Ekranı): Müşteri arama, aktif kampanya sayacı, XAI karar gerekçesi ve Anti-Gaming nedeniyle sayaca dahil edilmeyen işlemlerin nedenleriyle listelenmesi.
- `tests/Odea028.Tests`:
  - 8 KPI'ın tamamını (POS F1 => %92, Free-Rider <= %25, CUPED ROI => 1.35x, Reactivation => %6.5, Latency < 500 ms, MILP Monotonluk/Adillik %0 ihlal, PSI < 0.15, Anti-Gaming & Clawback %100 doğruluk) otomatik test eden xUnit test projesi.
```

---

## BÖLÜM 3: Adım Adım (6 Sprint) Sıralı Geliştirme Promptları

*(Claude Code veya GitHub Copilot'a modül modül, her adımı doğrulayarak yazdırmak için aşağıdaki 6 promptu sırasıyla verin.)*

---

### 🟢 PROMPT 1 (Adım 1): Solution Kurulumu, SQL Server 5 Çekirdekli Şema ve Sentetik Banka Veri Simülatörü

```text
Adım 1'i uyguluyoruz. `Odea028.CausalCampaignEngine.sln` çözümünü ve `Odea028.Core`, `Odea028.Infrastructure`, `Odea028.Tests` projelerini oluştur.

SQL Server Connection String:
"CONNECTION_STRING_BURAYA"

Bu adımda yapılacaklar:
1. `Odea028.Infrastructure` içinde `DatabaseInitializer.cs` sınıfını yaz. Bağlantı kurulduğunda veritabanı yoksa oluştursun ve aşağıdaki SQL Server tablolarını (Primary Key, Foreign Key ve performans indeksleriyle birlikte) eksiksiz kursun:
   - `CAMPAIGN_TEMPLATE` (`CampaignId`, `CampaignCode`, `CampaignName`, `CampaignType` [TotalSpend/Sectoral/Reactivation], `TargetSector`, `RewardType` [Puan/Cashback], `TotalBudget`, `MaxRewardPerCustomer`, `MarginRate`, `StartDate`, `EndDate`, `IsActive`, `KillSwitchTriggered`)
   - `CUSTOMER_PROFILE_MIS` (`CustomerId`, `CardNumberMasked`, `SegmentName` [Standart/Ayrıcalıklar Dünyası Gümüş/Altın/Özel], `AvailableCreditLimit`, `MisMonthlyNetProfit`, `IsDormant` [Son 90 gün inaktif mi], `HasKvkkIysConsent`, `PreferredRewardType`, `HistoricalBaselineSpend`, `CreatedAt`)
   - `INFLATION_INDEX_MONTHLY` (`YearMonth` [YYYYMM], `SectorName`, `CpiIndexValue`, `DeflatorMultiplier`)
   - `MERCHANT_MASTER_CACHE` (`RawMerchantKey` [PK, NVARCHAR(250)], `RawMcc`, `CleanedBrandName`, `ResolvedSector`, `SubMerchantProvider`, `ClassificationLayer` [1=Dict, 2=RegexPF, 3=ML], `ConfidenceScore`, `UpdatedAt`)
   - `CUSTOMER_OFFER_ASSIGNMENT` (`AssignmentId` [PK], `CampaignId`, `CustomerId`, `MicroBucketId` [1..20], `ExperimentGroup` [Treatment/Control_Holdout], `ExpectedOrganicSpend`, `PredictedUpliftSpend`, `AssignedTargetAmount`, `AssignedRewardAmount`, `RewardType`, `ExpectedNetIncrementalProfit`, `XaiReasonText`, `AlternativeOfferJson` [3 eşdeğer teklif], `CustomerSelectedOfferIndex`, `AssignedAt`)
   - `TRANSACTION_LEDGER_FEED` (`TransactionId` [PK], `AuthorizationCode`, `CustomerId`, `TransactionTimestamp`, `Amount`, `Currency`, `RawMcc_DE18`, `RawMerchantName_DE43`, `ResolvedBrand`, `ResolvedSector`, `IsReversalOrRefund`, `OriginalTransactionId`, `IsExcludedByAntiGaming`, `ExclusionReason`, `ProcessingLatencyMs`)
   - `OFFER_PROGRESS_TRACKER` (`TrackerId` [PK], `AssignmentId`, `CustomerId`, `CampaignId`, `TargetAmount`, `CurrentQualifiedSpend`, `RemainingAmount`, `ProgressPercentage`, `IsCompleted`, `CompletedAt`, `LastTransactionAt`, `RowVersion`)
   - `REWARD_FULFILLMENT_QUEUE` (`FulfillmentId` [PK], `TrackerId`, `CustomerId`, `CampaignId`, `RewardAmount`, `RewardType`, `Status` [PendingSettlement/Loaded/ClawedBack], `ScheduledLoadDate`, `ClawbackReason`, `UpdatedAt`)

2. `SyntheticBankDataSeeder.cs` sınıfını yaz (`SqlBulkCopy` kullanarak yüksek hızda veri bassın):
   - Son 12 aya ait `INFLATION_INDEX_MONTHLY` verisini (aylık enflasyon artışlarıyla) oluştur.
   - En az 2.000 kredi kartı müşterisi (`CUSTOMER_PROFILE_MIS`) oluştur: İçinde yüksek organik harcama yapan "Free-Rider" müşteriler, kampanya elastikiyeti yüksek "Persuadable" müşteriler, son 90 gündür harcama yapmayan "Inaktif/Uyuyan (Dormant)" kartlar ve Ayrıcalıklar Dünyası segmentleri bulunsun.
   - Geçmiş kampanya ve işlem kayıtlarını (`TRANSACTION_LEDGER_FEED`) üret: İçinde temiz ulusal zincirler (`MIGROS`, `SHELL`, `THY`), ödeme kuruluşu arkasındaki gizli markalar (`IYZICO*TRENDYOL.COM`, `PARAM*GETIR PERAKENDE`, `PAYTR*HEPSIBURADA`, `SIPAY*ZARA TR`), yanlış MCC atanmış uzun kuyruk POS metinleri ve nakit benzeri MCC (`6011`, `7995`) işlemleri bulunsun.

3. `Odea028.Tests` içinde veritabanı şemasının kurulduğunu ve sentetik verinin SQL Server'a başarıyla yazıldığını doğrulayan entegrasyon testini yaz ve `dotnet test` ile çalıştır.
```

---

### 🟢 PROMPT 2 (Adım 2): 3 Katmanlı POS İşlem Zenginleştirme Motoru (İP-2)

```text
Adım 2'yi uyguluyoruz. `src/Odea028.PosEnrichment` projesini oluştur ve `Odea028.CausalCampaignEngine.sln` çözümüne ekle.

Bu adımda ISO 8583 `DE-18 (MCC)` ve `DE-43 (Üye İşyeri Adı/Lokasyonu)` alanlarındaki kategori kirliliğini çözen 3 Katmanlı Hibrit Semantik POS Sınıflandırıcıyı (`PosEnrichmentEngine.cs`) %100 C# ile kodla:

1. Önbellek Kontrolü (`MERCHANT_MASTER_CACHE`):
   - İşyeri metni daha önce çözümlenmişse bellek içi `ConcurrentDictionary` / SQL Server `MERCHANT_MASTER_CACHE` tablosundan `< 1 ms` içinde `O(1)` karmaşıklıkla döndür.

2. Katman 1 — Ulusal Zincir ve Bilinen Marka Sözlüğü (`DeterministicBrandDictionary.cs`):
   - Türkiye'deki başlıca market, akaryakıt, giyim, elektronik, havayolu, restoran ve e-ticaret zincirlerini (`MIGROS`, `BIM`, `A101`, `CARREFOUR`, `SHELL`, `OPET`, `PETROL OFISI`, `LC WAIKIKI`, `BOYNER`, `TEKNOSA`, `MEDIAMARKT`, `THY`, `PEGASUS`, `STARBUCKS` vb.) `FrozenDictionary` ile eşleştir.

3. Katman 2 — Ödeme Kuruluşu (PF / Aggregator) Alt-İşyeri Ayrıştırıcı (`PaymentFacilitatorParser.cs`):
   - C# `[GeneratedRegex]` kullanarak `IYZICO*`, `PARAM*`, `PAYTR*`, `SIPAY*`, `MOKA*`, `ODEAL*`, `CRAFTGATE*`, `PAYCELL*` gibi ödeme kuruluşu ön eklerini, şehir/ülke kodlarını ve POS terminal numaralarını temizle; alt-işyeri adını çıkar ve bilinen markayla veya Katman 3 ile eşleştir.

4. Katman 3 — ML.NET Yerel Semantik Metin Sınıflandırıcı (`LocalMlMerchantClassifier.cs`):
   - `Microsoft.ML` (`MLContext.Transforms.Text.FeaturizeText` karakter ve kelime n-gram'ları + `RawMcc` + `LightGbmMulticlass`) kullanarak uzun kuyruk (long-tail) ve gürültülü Türkçe POS işyeri adlarını (`Market & Gıda`, `Akaryakıt`, `Giyim & Tekstil`, `Elektronik`, `Restoran & Kafe`, `Seyahat & Konaklama`, `E-Ticaret & Pazaryeri`, `Sağlık & Kozmetik`, `Nakit Benzeri / Yasaklı`) sektörlerine sınıflandıran modeli eğit ve yeni çözümlenen işyerlerini SQL Server `MERCHANT_MASTER_CACHE` tablosuna kaydet.

5. `Odea028.Tests` içinde 500+ zorlu ödeme kuruluşu ve bozuk POS işlem metni içeren altın standart (ground-truth) test seti üzerinde sınıflandırma doğruluğunu ölçen bir xUnit testi yaz. Macro F1-Score değerinin `=> 0.92` (`%92,0`) olduğunu ve önbellek okuma süresinin `< 1 ms` olduğunu `dotnet test` ile doğrula.
```

---

### 🟢 PROMPT 3 (Adım 3): Enflasyon-Düzeltmeli Feature Store ve ML.NET Robinson R-Learner Nedensel Uplift Motoru (İP-3A & İP-4)

```text
Adım 3'ü uyguluyoruz. `src/Odea028.CausalAi` projesini oluştur ve `Odea028.CausalCampaignEngine.sln` çözümüne ekle.

Bu adımda yüksek enflasyon ve düşük örneklem belirsizliğini çözen Nedensel Yapay Zekâ çekirdeğini %100 C# (`Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender`, `MathNet.Numerics`) ile kodla:

1. Enflasyon-Düzeltmeli Feature Store (`InflationAdjustedFeatureStore.cs`):
   - SQL Server üzerinde müşterilerin son 3, 6 ve 12 aylık harcamalarını `INFLATION_INDEX_MONTHLY` tablosundaki deflatör katsayısıyla reel TL'ye çevir.
   - Her müşterinin portföy içindeki göreli harcama yüzdelik dilimini (`RelativeSpendQuantile` - `PERCENT_RANK()`), harcama trend eğimini (`SpendTrendSlope`), işlem sıklığını (`TxFrequency`), aktif sektör çeşitliliğini, MIS kârlılık skorunu ve kullanılabilir boş limit oranını çıkaran öznitelik vektörünü (`CustomerCausalFeatureVector`) üret.

2. İki Aşamalı Robinson Ayrıştırmalı R-Learner ve Bayesyen Küçültme Motoru (`RobinsonRLearnerUpliftEngine.cs`):
   - 1. Aşama (Organik Harcama / Baseline Modeli `m_hat(X)` ve Propensity Modeli `e_hat(X)`):
     - `MLContext.Regression.Trainers.LightGbm` ile müşterinin kampanya almasa dahi gelecek ay yapacağı Beklenen Organik Harcamayı (`ExpectedOrganicSpend`) tahmin et.
     - `MLContext.BinaryClassification.Trainers.LightGbm` ile kampanya alma olasılığını (`PropensityScore`, `[0.05, 0.95]` aralığında kırpılmış/clipped) tahmin et.
   - 2. Aşama (Sapma Artıkları Üzerinden Bireysel Kampanya Etkisi `tau_hat(X)`):
     - Harcama artığını `Y_residual = ActualSpend - ExpectedOrganicSpend` ve müdahale artığını `W_residual = TreatmentIndicator - PropensityScore` olarak hesapla.
     - Sözde-hedef `PseudoOutcome = Y_residual / W_residual` ve örneklem ağırlığı `Weight = W_residual * W_residual` kullanarak ağırlıklı `LightGbm` regresyon modeli eğit.
     - Düşük örneklem gürültüsünü bastırmak için `MathNet.Numerics` ile Empirik Bayesyen Küçültme (Empirical Bayes Shrinkage) uygula: Gözlem sayısı az veya varyansı yüksek tahminleri segment ortalamasına doğru küçült.
   - İnaktif (Uyuyan) Kart Uyandırma Modeli (`DormantCardReactivationModel.cs`):
     - `IsDormant = 1` olan müşteriler için farklı başlangıç hedef/ödül seviyelerinde 60 günlük kalıcı aktifleşme olasılığını ve beklenen harcamayı hesapla.

3. Sektörel Öneri (`SectorRecommendationEngine.cs`) ve Contextual Bandit (`ThompsonSamplingBandit.cs`):
   - `Microsoft.ML.Recommender` (`MatrixFactorizationTrainer`) ile müşteri-sektör etkileşim matrisini çözerek müşterinin harcama potansiyeli en yüksek yeni sektörü öner.
   - `MathNet.Numerics.Distributions.Beta` kullanarak geçmiş veride az denenmiş hedef/ödül oranlarını keşfeden Thompson Sampling Bağlamsal Çok Kollu Haydut modülünü yaz.

4. Açıklanabilir Yapay Zekâ (`XaiExplanationService.cs`) ve PSI Drift / HDBSCAN (`DriftAndClusteringService.cs`):
   - ML.NET `CalculateFeatureContributions` ile her müşteri için kararın arkasındaki en güçlü 3 özniteliği çıkar ve Odea Mobil'de müşteriye gösterilecek yasal/etik uyumlu Türkçe gerekçeye dönüştür (ör. *"Son 3 aydaki düzenli Market & Gıda harcama trendiniz ve Ayrıcalıklar Dünyası Altın segment avantajınız dikkate alınarak size özel oluşturulmuştur."*).
   - Enflasyon kayması için aylık `PSI (Population Stability Index)` hesaplayıcısını (`PSI < 0.15` kontrolü) ve HDBSCAN yoğunluk tabanlı davranışsal mikro-küme keşif algoritmasını kodla.

5. `Odea028.Tests` içinde R-Learner modelinin "Free-Rider" müşterileri doğru tespit ettiğini (organik harcaması yüksek ama uplift'i düşük müşterilere gereksiz ödül önermediğini), XAI Türkçe gerekçesinin %100 üretildiğini ve PSI drift hesabının çalıştığını doğrulayan xUnit testlerini yaz ve çalıştır.
```

---

### 🟢 PROMPT 4 (Adım 4): `Google.OrTools` MILP Bütçe/Adillik Optimizatörü ve 20 Mikro-Kova Atayıcı (İP-3B)

```text
Adım 4'ü uyguluyoruz. `src/Odea028.Optimization` projesini oluştur, `Google.OrTools` NuGet paketini kur ve `Odea028.CausalCampaignEngine.sln` çözümüne ekle.

Bu adımda her müşteri için "Teklif sunulmalı mı, hangi hedef ve hangi ödül atanmalı?" problemini tüm banka kısıtları altında çözen Karma Tamsayılı Doğrusal Programlama (MILP) motorunu (`CampaignPortfolioMilpOptimizer.cs`) kodla:

1. Aday Teklif Paketi ve 20 Mikro-Kova Üretimi (`MicroBucketGenerator.cs`):
   - Operasyonel karmaşayı önlemek için portföydeki harcama hedeflerini ve ödül oranlarını 20 standart Mikro-Kovaya (`MicroBucketId: 1..20`) ayır (inaktif uyandırma kovaları, sektörel geçiş kovaları ve harcama artışı/upsell kovaları).
   - Her müşteri `i` ve aday kova `k` için `Odea028.CausalAi` motorundan gelen:
     - `ExpectedOrganicSpend[i]`
     - `PredictedIncrementalUplift[i, k]`
     - `TargetAmount[i, k]`
     - `RewardCost[i, k]`
     - `NetEconomicValue[i, k] = (PredictedIncrementalUplift[i, k] * MarginRate) - RewardCost[i, k] + (MisProfitabilityWeight * MisBonus[i])` değerlerini hazırla.

2. `Google.OrTools.LinearSolver` (`Solver.CreateSolver("SCIP")` veya `"CBC_MIXED_INTEGER_PROGRAMMING"`) ile Kısıtlı Optimizasyon:
   - Karar Değişkeni: `x[i, k] in {0, 1}` (Müşteri `i`'ye teklif paketi `k` atandıysa 1, aksi halde 0).
   - Kısıt 1 (Tekil Atama): Her müşteri için `Sum_k(x[i, k]) <= 1`. Eğer tüm paketler için `NetEconomicValue[i, k] <= 0` ise veya müşteri "Free-Rider" ise `x[i, k] = 0` olur (teklif atanmaz, bütçe korunur).
   - Kısıt 2 (Toplam Bütçe Kısıtı): `Sum_i Sum_k (RewardCost[i, k] * x[i, k]) <= Campaign.TotalBudget`.
   - Kısıt 3 (Boş Kart Limiti ve Tavan Ödül Kısıtı): `TargetAmount[i, k] <= Customer.AvailableCreditLimit * 0.80` ve `RewardCost[i, k] <= Campaign.MaxRewardPerCustomer`.
   - Kısıt 4 (MIS Kârlılık ve Segment Uyumu): Negatif MIS kârlılığı olan veya KVKK/İYS izni olmayan müşterilere bütçe yakıcı ödül atanamaz; Ayrıcalıklar Dünyası segment taban ödül kuralları korunur.
   - Kısıt 5 (Algoritmik Adillik / Monotonluk Kısıtı): Aynı kampanya tipinde sıralı mikro-kovalarda daha yüksek harcama hedefi (`TargetAmount`) olan bir kovanın mutlak ödül tutarı (`RewardCost`), daha düşük hedefli bir kovadan kesinlikle düşük olamaz (`Target_a > Target_b => Reward_a => Reward_b`).
   - Eşdeğer Kârlılıklı 3 Teklif Seçeneği (Active Preference Learning): Atanan ana teklifin yanında, banka için beklenen net ekonomik katkısı (`NetEconomicValue`) birbirine yakın olan 3 alternatif kurguyu (1: Düşük Hedef + Puan, 2: Dengeli Hedef + Nakit İade, 3: Sektörel Özel Avantaj) `AlternativeOfferJson` olarak üret.

3. Tabakalı Rastgeleleştirilmiş Kontrol Grubu (`StratifiedHoldoutSplitter.cs`):
   - Teklif almaya uygun müşterileri harcama kuantil dilimi ve segment bazında tabakalayarak (Stratified) `%90 Müdahale (Treatment)` ve `%10 Kontrol (Control_Holdout)` grubuna ayır ve atamaları SQL Server `CUSTOMER_OFFER_ASSIGNMENT` ve `OFFER_PROGRESS_TRACKER` tablolarına kaydet.

4. `Odea028.Tests` içinde MILP çözücüsünün bütçeyi aşmadığını, Free-Rider oranını `<= %25` seviyesine indirdiğini, monotonluk (adillik) kısıtını `%0` ihlalle sağladığını ve çözüm süresinin `< 15 dakika` (test portföyünde birkaç saniye) olduğunu doğrulayan xUnit testlerini yaz ve çalıştır.
```

---

### 🟢 PROMPT 5 (Adım 5): Gerçek Zamanlı Sayaç (`< 500 ms`), Anti-Gaming, Clawback ve CUPED Deney Motoru (İP-3C)

```text
Adım 5'i uyguluyoruz. Gerçek zamanlı provizyon işleme, Anti-Gaming filtreleri, ödül geri alma (Clawback) ve CUPED net ekonomik katkı doğrulama servislerini kodla.

Bu adımda yapılacaklar:
1. Gerçek Zamanlı Provizyon ve Sayaç Servisi (`RealtimeOfferTrackerService.cs`):
   - ISO 8583 `0100/0200` kart provizyon mesajı (`TransactionAuthorizationRequest`) geldiğinde `Dapper` ile yüksek hızda (`< 500 ms`) çalışacak akışı kodla:
     - Adım A: `PosEnrichmentEngine` ile işlemin gerçek marka ve sektörünü `< 5 ms` içinde çözümle.
     - Adım B (Anti-Gaming Kural 1 - Yasaklı/Nakit Benzeri MCC): İşlem MCC kodu nakit çekim, kripto, bahis, döviz bürosu, kuyum veya fatura ödeme/para transferi (`6011`, `6012`, `6051`, `7995`, `4829`) ise sayaca dahil etme (`IsExcludedByAntiGaming = 1`, `ExclusionReason = "Nakit Benzeri / Kampanya Dışı MCC"`).
     - Adım C (Anti-Gaming Kural 2 - Bölünmüş İşlem / Split-Transaction Tespiti): Aynı müşteri aynı üye işyerinde son 30 dakika içinde ardışık işlem yapmışsa bölünmüş işlem suistimali olarak işaretle ve işlem adedi bazlı kampanya sayacını artırma (`ExclusionReason = "30 Dk İçinde Aynı İşyerinde Bölünmüş İşlem"`).
     - Adım D (Sektör ve Tarih Kontrolü): İşlem müşterinin aktif kampanyasının hedef sektörü ve tarih aralığıyla uyumluysa `OFFER_PROGRESS_TRACKER` tablosundaki `CurrentQualifiedSpend`, `RemainingAmount` ve `ProgressPercentage` alanlarını atomik olarak güncelle.
     - Adım E (Hedef Tamamlama & Ödül Kuyruğu): `CurrentQualifiedSpend => TargetAmount` olduğu anda `IsCompleted = 1` yap ve takas (settlement) sonrası yüklenmek üzere `REWARD_FULFILLMENT_QUEUE` tablosuna kayıt aç.

2. İptal / İade ve Ödül Geri Alma (`Clawback`) Akışı (`ProcessReversalOrRefundAsync`):
   - Müşteri harcama hedefini tamamlayıp ödül kazandıktan sonra işlemini iptal/iade ederse (`IsReversalOrRefund = 1`), `OFFER_PROGRESS_TRACKER` sayacından iade tutarını düş.
   - Eğer güncel harcama `TargetAmount` altına düşerse `IsCompleted = 0` yap ve `REWARD_FULFILLMENT_QUEUE` tablosundaki ödül kaydını `Status = 'ClawedBack'` (`ClawbackReason = "İade Kaynaklı Hedef Altına Düşüş"`) olarak güncelle.

3. CUPED (Deney Öncesi Veriyle Varyans Azaltma) Net Ekonomik Katkı Doğrulayıcı (`CupedExperimentEvaluator.cs`):
   - `Treatment` ve `Control_Holdout` gruplarının deney öncesi harcamasını (`X_pre` = `HistoricalBaselineSpend`) ortak değişken (covariate) olarak kullan:
     - `Theta = Cov(Y_post, X_pre) / Var(X_pre)`
     - `Y_cuped = Y_post - Theta * (X_pre - Mean(X_pre))`
   - Klasik yöntemle ve ODEA-028 Nedensel AI yöntemiyle elde edilen `Incremental ROI = (İlave Harcama * Marj) / Ödül Maliyeti` oranını karşılaştır ve `=> 1.35x` (`+%35` net ilave katkı artışı) hedefini raporla.

4. `Odea028.Tests` içinde P95 işlem gecikmesinin `< 500 ms` olduğunu, bölünmüş işlem ve nakit benzeri MCC'lerin `%100` yakalandığını, iade durumunda `Clawback` mekanizmasının çalıştığını ve `CUPED` Incremental ROI değerinin `=> 1.35x` çıktığını doğrulayan xUnit testlerini yaz ve çalıştır.
```

---

### 🟢 PROMPT 6 (Adım 6): `ASP.NET Core + Blazor` 3'ü 1 Arada Canlı Demo, Back-Office ve Çağrı Merkezi Uygulaması (İP-5 & İP-6)

```text
Adım 6'yı uyguluyoruz. `src/Odea028.Web` (`ASP.NET Core 8.0 + Blazor Web App Interactive Server`) projesini oluştur ve tüm arka plan motorlarını (`PosEnrichment`, `CausalAi`, `Optimization`, `Infrastructure`) Dependency Injection ile bağla.

Bu adımda yapılacaklar:
1. REST API Uç Noktaları (`Controllers/CampaignEngineController.cs`):
   - `POST /api/v1/transactions/authorize`: Gerçek zamanlı ISO 8583 kart provizyon mesajı alır, POS zenginleştirme + Anti-Gaming + Sayaç güncellemesi yapar ve güncel sayaç durumu ile işlem gecikmesini (`LatencyMs`) döndürür.
   - `POST /api/v1/transactions/reversal`: İptal/iade mesajı alır, sayacı düşürür ve gerekirse `Clawback` çalıştırır.
   - `POST /api/v1/campaigns/{id}/optimize`: R-Learner Uplift + Google.OrTools MILP optimizasyonunu çalıştırır.
   - `POST /api/v1/offers/{assignmentId}/select-preference`: Müşterinin Odea Mobil'de 3 eşdeğer tekliften seçtiği kurguyu kaydeder ve Contextual Bandit tercih ağırlıklarını günceller.
   - `POST /api/v1/campaigns/{id}/kill-switch`: Acil durdurma (Kill-Switch) tetikler.

2. 3'ü 1 Arada Modern Blazor Web Arayüzü (`Components/Pages/Home.razor` veya sekmeli Dashboard):
   Odeabank kurumsal renklerinde, tek ekranda 3 sekmeden oluşan interaktif bir komuta merkezi tasarla:
   - **SEKME 1: 📱 Odea Mobil Müşteri Simülatörü**
     - Müşteri seçme açılır listesi (Persuadable, Free-Rider, Uyuyan Kart, Ayrıcalıklar Dünyası Altın müşterileri).
     - Mobil telefon çerçevesi içinde:
       - **Kişiye Özel Teklif Kartı:** Atanan harcama hedefi, ödül tutarı/tipi ve XAI tarafından üretilen Türkçe gerekçe kutusu.
       - **Avantajını Kendin Seç (Active Preference Learning):** Banka açısından eşdeğer kârlılıktaki 3 teklif kartı (Müşteri tıkladığında anında seçili kurguya geçen interaktif butonlar).
       - **Canlı Hedef Sayacı (Progress Bar):** Harcanan tutar, kalan tutar, yüzde çubuğu ve işlem geçmişi.
       - **Canlı POS İşlem Simülatörü:** İşyeri adı (ör. `IYZICO*TRENDYOL.COM`, `MIGROS`, `KUMARHANE/BET` veya `30 dk içinde 2. Bölünmüş İşlem`), MCC ve Tutar girip **"💳 Kart İşlemi Geçir"** veya **"↩️ İade (Refund) Geçir"** butonuna basıldığında `< 500 ms` içinde sayacın canlı dolmasını, ödül kazanımını veya Anti-Gaming/Clawback uyarısını anlık göster.
   - **SEKME 2: 🎛️ Back-Office Yönetim, CUPED Analitik & Kill-Switch Paneli**
     - 8 KPI Özet Kartı (POS F1 Doğruluğu, Free-Rider Oranı, CUPED Incremental ROI, Uyuyan Kart Dönüşümü, P95 Gecikme, MILP Çözüm Süresi, Aylık PSI Drift, Kural İhlali).
     - **"🚀 R-Learner + MILP Portföy Optimizasyonunu Çalıştır"** butonu ve 20 Mikro-Kova dağılım tablosu.
     - HDBSCAN Yoğunluk Tabanlı Mikro-Küme Keşif Özeti ve **"🛑 ACİL DURDUR (KILL-SWITCH)"** butonu.
   - **SEKME 3: 🎧 Çağrı Merkezi Müşteri Gerekçe & Denetim Ekranı**
     - Müşteri temsilcisinin müşteriye neden bu hedefin atandığını (XAI öznitelik katkılarıyla), hangi işlemlerinin kampanya sayacına sayıldığını ve hangi işlemlerinin Anti-Gaming kuralları (bölünmüş işlem / nakit benzeri MCC / iade clawback) nedeniyle kapsam dışı kaldığını şeffafça gördüğü ekran.

3. Tüm çözümü `dotnet build` ve `dotnet test` ile doğrula; hiçbir hata olmadığından emin ol.
```
