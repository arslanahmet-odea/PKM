# PROJE 1 PROMPTU: `CausalCampaign.Domain` & `CausalCampaign.sln` (Clean Architecture Çekirdek Katmanı)

*(Bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0 ve Microsoft SQL Server konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **1. Aşamasına (`CausalCampaign.Domain` ve `CausalCampaign.sln` kurulumu)** başlıyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`
Veritabanı Referans Scripti: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka\CausalCampaign_Prompts\00_Database\CausalCampaignDb_Setup_And_Seed.sql`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını kullan. Tüm çözüm tek bir `CausalCampaign.sln` dosyasında toplanacaktır.
   - Sorumlulukların ayrılığı (Separation of Concerns) ilkesine göre katmanları kesin sınırlarla ayır (`CausalCampaign.Domain` hiçbir dış kütüphaneye veya altyapıya bağımlı olmamalıdır).
   - Veritabanı adı `CausalCampaignDb`'dir. Kodun, namespace'lerin, yorumların veya verilerin hiçbir yerinde kurum kodu/özel proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY (Don't Repeat Yourself) ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>` ve `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` aktif olmalıdır. `dynamic` veya belirsiz `object` tiplerinden kaçın. İş kuralları dönüşlerinde güçlü tipli `Result<T>` ve `DomainError` desenini kullan.
   - Fonksiyonları kısa, tek bir işe odaklanan (Single Responsibility) ve okunabilir şekilde yaz; anlamlı değişken isimleri kullan.

3. **Hata Yönetimi ve Güvenlik:**
   - Domain seviyesinde istisna fırlatmak (exception-driven control flow) yerine `Result<T>` / `Error` nesneleri ve açık domain doğrulama kuralları kullan.
   - Hassas verileri (Connection String, API anahtarları, şifreler) asla kod içine gömme; `.env` / Environment Variables ve `IOptions<T>` yapılandırma doğrulamasını (`ValidateOnStart`) temel al.

4. **Test ve Dokümantasyon:**
   - Birim testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest: `[TestClass]`, `[TestMethod]`, `[DataRow]`)** ve **`Moq`** kullan.
   - Tüm public sınıflar, arayüzler ve karmaşık domain fonksiyonları için kısa ve net **XML Documentation (`/// <summary>`)** açıklamaları yaz.
   - Hiçbir yerde `// TODO` veya `NotImplementedException` bırakma.

---

###BU AŞAMADA (`CausalCampaign.Domain`) İNŞA EDİLECEKLER

1. **Solution Kurulumu (`CausalCampaign.sln`):**
   - `src/CausalCampaign.Domain` (`net10.0` Class Library)
   - `tests/CausalCampaign.UnitTests` (`net10.0` MSTest + Moq Test Project)
2. **Ortak Çekirdek Yapılar (`Common/`):**
   - `Result`, `Result<T>`, `DomainError`, `ErrorType` (Validation, Conflict, NotFound, AntiGamingViolation, KillSwitchActive), `BaseEntity`.
3. **8 Çekirdek Tablo Varlığı (`Entities/`) — Encapsulated & Rich Domain Models:**
   - `CampaignTemplate`: Bütçe kontrolü, `TriggerKillSwitch(string reason)` ve aktiflik doğrulama davranışlarıyla.
   - `CustomerProfileMis`: Kredi limiti kontrolü (`GetMaxAllowedTargetAmount() => AvailableCreditLimit * 0.80m`), MIS kârlılık ve uyuyan kart (`IsDormant`) davranışlarıyla.
   - `InflationIndexMonthly`: Sektörel TÜFE ve reel deflatör (`DeflatorMultiplier`) hesaplama davranışıyla.
   - `MerchantMasterCache`: 3 katmanlı POS sınıflandırma önbellek varlığı.
   - `CustomerOfferAssignment`: R-Learner + MILP teklif ataması, 20 mikro-kova, 3 eşdeğer alternatif teklif seçimi (`SelectAlternativeOffer(byte optionIndex)`) ve XAI gerekçesiyle.
   - `TransactionLedgerFeed`: ISO 8583 `DE-18 MCC` ve `DE-43 İşyeri Adı` provizyon kaydı ve Anti-Gaming denetim izi.
   - `OfferProgressTracker`: `< 500 ms` gerçek zamanlı sayaç, `ApplyQualifiedTransaction(decimal amount)`, `ApplyReversalOrRefund(decimal amount)` ve `byte[] RowVersion` iyimser eşzamanlılık (Optimistic Concurrency) token'ı ile.
   - `RewardFulfillmentQueue`: Ödül hak ediş ve iade kaynaklı ödül geri alma (`TriggerClawback(string reason)`) durumuyla.
   - `CustomerCausalFeatureView`: `vw_CustomerCausalFeatures` görünümü karşılığı.
4. **Değer Nesneleri, Sabitler ve Enum'lar (`ValueObjects/`, `Enums/`, `Constants/`):**
   - `CampaignType`, `RewardType` (`RewardPoints`, `Cashback`), `PrestigeSegment` (`Standard`, `Silver`, `Gold`, `Private`), `SpendElasticityPersona`, `ExperimentGroup`, `FulfillmentStatus`, `PosClassificationLayer`.
   - `AntiGamingConstants`: Yasaklı/nakit benzeri MCC listesi (`6011`, `6012`, `6051`, `7995`, `4829`) ve `SplitTransactionWindowMinutes = 30`.
   - `KpiThresholds`: 8 KPI hedef sabitleri (`MinPosMacroF1 = 0.92m`, `MaxFreeRiderRatio = 0.25m`, `MinIncrementalRoi = 1.35m`, `MinReactivationRate = 0.065m`, `MaxLatencyMs = 500m`, `MaxPsiDrift = 0.15m`).
5. **Domain & Port Arayüzleri (`Interfaces/`):**
   - `ICausalCampaignDbContext`, `IPosEnrichmentService`, `ICausalUpliftEngine`, `IMilpPortfolioOptimizer`, `IRealtimeOfferTrackerService`, `ICupedExperimentEvaluator`, `IXaiExplanationService`, `IDriftAndClusteringService`.
6. **Birim Testler (`tests/CausalCampaign.UnitTests/DomainTests/`):**
   - `MSTest` ve `Moq` kullanarak `OfferProgressTracker` sayaç artırma/tamamlama, iade sonrası `Clawback` tetikleme, `CampaignTemplate` Kill-Switch, `CustomerProfileMis` %80 boş limit kuralı ve Anti-Gaming yasaklı MCC kurallarını doğrulayan en az 12 birim testi.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.Domain` ve `CausalCampaign.UnitTests` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `.NET 10.0`, `MSTest`, `Moq` vb.
3. **Mimari Kararların Özeti:** Clean Architecture sınırları, Rich Domain Model, `Result<T>` hata deseni ve Optimistic Concurrency kararların.

Sonunda **"1. Aşama (`CausalCampaign.Domain`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` komutlarıyla `%100` doğrulayıp incelemem için dur.
```
