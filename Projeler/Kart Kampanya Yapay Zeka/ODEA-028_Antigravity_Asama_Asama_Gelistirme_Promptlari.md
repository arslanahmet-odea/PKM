# ODEA-028: Antigravity İçin Aşama Aşama (Proje Bazlı) Geliştirme Prompt Kitabı

Bu doküman, **ODEA-028 (Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru)** projesini **farklı zamanlarda, aşama aşama ve her bir proje modülü bittiğinde sizin incelemenizden geçecek şekilde** doğrudan **Antigravity**'ye (bana) yaptırmanız için hazırlanmıştır.

---

## ⚙️ Sabit Mimari ve Teknoloji Kuralları (Tüm Aşamalarda Geçerli)

* **Hedef Framework:** `.NET 10.0` (`net10.0`) ve **`ASP.NET Core 10.0`**
* **Veritabanı ve ORM:** **Microsoft SQL Server** + **`Entity Framework Core 10.0`** (`Microsoft.EntityFrameworkCore.SqlServer`). *(Dapper kesinlikle kullanılmayacaktır; `< 500 ms` gerçek zamanlı sayaç performansı için EF Core `EF.CompileAsyncQuery`, `AsNoTracking()` ve `RowVersion` Optimistic Concurrency kullanılacaktır).*
* **Birim Test ve Mocking:** **`MSTest`** (`Microsoft.VisualStudio.TestTools.UnitTesting` / `MSTest.TestFramework`) ve **`Moq`** (`Mock<T>`). *(xUnit kullanılmayacaktır).*
* **Yapay Zekâ ve Optimizasyon Kütüphaneleri:** `Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender`, `Google.OrTools`, `MathNet.Numerics`.
* **Çalışma Prensibi (2 Fazlı Onay Akışı):** Her prompt çalıştırıldığında Antigravity **önce tek satır kod yazmadan detaylı uygulama planını sunup onayınızı bekleyecek**, siz **"Onaylıyorum, kodla"** dedikten sonra kodu ve `MSTest + Moq` testlerini yazıp incelemeniz için duracaktır.

---

## 🗄️ AŞAMA 0: Sıfırdan Veritabanı, Tablo ve Veri Kurulum Scripti

Veritabanını, 8 çekirdek tabloyu, indeksleri, `vw_CustomerCausalFeatures` görünümünü ve gerçekçi başlangıç verilerini sıfırdan kuran T-SQL scripti çalışma klasörünüze kaydedilmiştir:
* **SQL Script Dosyası:** [ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql](file:///c:/Users/obu50550/Documents/PKM/Projeler/Kart%20Kampanya%20Yapay%20Zeka/ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql)

*(İsterseniz bu scripti SSMS'te kendiniz çalıştırabilir veya **Prompt 1 / Prompt 2** sırasında bana Connection String vererek benim SQL Server üzerinde otomatik çalıştırmamı isteyebilirsiniz.)*

---

## 🧩 PROMPT 1 (1. Proje): `Odea028.Core` — Solution Kurulumu, Domain Modelleri, İş Kuralları ve Arayüzler

*(İlk oturumda aşağıdaki promptu kopyalayıp bana yapıştırın.)*

```text
ODEA-028 ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin 1. Aşamasına (1. Projesine) başlıyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`
Referans Dokümanlar:
- `ODEA-028_Proje_Bilgi_Formu.md`
- `ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql`

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. Hedef Framework `.NET 10.0` (`net10.0`) olacaktır.
2. Veri erişiminde Dapper KULLANILMAYACAK, yalnızca `Entity Framework Core 10.0` kullanılacaktır.
3. Tüm testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest: `[TestClass]`, `[TestMethod]`) ve `Moq` kullanılacaktır.
4. DİKKAT - ÖNCE PLAN SUN: Şimdi hemen kod yazma! Önce bu aşamada oluşturacağın `.sln` yapısını, `Odea028.Core` içindeki tüm sınıfları, metot imzalarını ve `Odea028.Tests` içindeki MSTest + Moq test senaryolarını bana detaylı bir PLAN olarak sun ve "Onaylıyor musunuz?" diye sor. Ben onay verdikten sonra kodları yaz, `dotnet build` ve `dotnet test` ile doğrula ve incelemem için dur.

BU AŞAMADA (1. PROJEDE) İNŞA EDİLECEKLER:
1. `Odea028.CausalCampaignEngine.sln` çözümünü, `src/Odea028.Core` (`net10.0` Class Library) ve `tests/Odea028.Tests` (`net10.0` MSTest + Moq Test Project) projelerini oluştur.
2. `Odea028.Core/Entities` altında `ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql` içindeki 8 tabloyla birebir uyumlu Entity sınıflarını oluştur:
   - `CampaignTemplate`, `CustomerProfileMis`, `InflationIndexMonthly`, `MerchantMasterCache`, `CustomerOfferAssignment`, `TransactionLedgerFeed`, `OfferProgressTracker` (`RowVersion` byte[] concurrency token dahil), `RewardFulfillmentQueue` ve `CustomerCausalFeatureView` (`vw_CustomerCausalFeatures` karşılığı).
3. `Odea028.Core/Enums` ve `Odea028.Core/Constants` altında:
   - Kampanya tipleri (`TotalSpendUpsell`, `SectoralCrossSell`, `DormantReactivation`), Ödül tipleri (`OdeaPuan`, `Cashback`), Ayrıcalıklar Dünyası segment kuralları, Anti-Gaming nakit benzeri yasaklı MCC listesi (`6011`, `6012`, `6051`, `7995`, `4829`), 30 dakika bölünmüş işlem (split-transaction) eşiği ve 8 KPI hedef sabitlerini tanımla.
4. `Odea028.Core/DTOs` ve `Odea028.Core/Interfaces` altında:
   - `IPosEnrichmentService`, `ICausalUpliftEngine`, `IMilpPortfolioOptimizer`, `IRealtimeOfferTrackerService`, `ICupedExperimentEvaluator`, `IXaiExplanationService` ve EF Core repository/DbContext arayüzlerini (`IOdea028DbContext`) tüm girdi/çıktı DTO'larıyla eksiksiz tanımla.
5. `Odea028.Tests` içinde `MSTest` ve `Moq` kullanarak domain iş kurallarını, Anti-Gaming sabitlerini ve arayüz kontratlarını doğrulayan birim testlerini yaz.
```

---

## 🧩 PROMPT 2 (2. Proje): `Odea028.Infrastructure` — EF Core 10.0 Veri Katmanı, Derlenmiş Sorgular ve Veri Üretici

*(1. Projeyi inceleyip onayladıktan sonra, 2. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 2. Aşamasına (`src/Odea028.Infrastructure` — Entity Framework Core 10.0 Veri Katmanı ve Sentetik Banka Veri Simülatörü) geçiyoruz.

SQL Server Connection String:
"CONNECTION_STRING_BURAYA"

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0` (`net10.0`) ve `Microsoft.EntityFrameworkCore.SqlServer` kullanılacaktır (Dapper kesinlikle kullanılmayacaktır).
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce `Odea028DbContext` tasarımını, `IEntityTypeConfiguration<T>` sınıflarını, `< 500 ms` gerçek zamanlı sayaç için kullanacağın `EF.CompileAsyncQuery` derlenmiş sorgularını, `ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql` scriptini çalıştıracak kurucu sınıfı ve MSTest + Moq test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (2. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.Infrastructure` projesini oluşturup `Odea028.Core` projesine referans ver.
2. `Odea028DbContext` sınıfını ve 8 tablo + `vw_CustomerCausalFeatures` görünümü için `IEntityTypeConfiguration<T>` Fluent API konfigürasyonlarını yaz:
   - Decimal hassasiyetleri (`decimal(18,2)`, `decimal(12,6)`, `decimal(6,4)`), Foreign Key ilişkileri, indeksler ve `OfferProgressTracker.RowVersion` (`IsRowVersion()`) iyimser eşzamanlılık (Optimistic Concurrency) ayarlarını eksiksiz tanımla.
3. `SqlDatabaseBootstrapper.cs` sınıfını yaz:
   - Bağlantı kurulduğunda `ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql` dosyasını (`GO` bloklarını ayrıştırarak) SQL Server üzerinde çalıştırıp veritabanını, tabloları, view'ı ve başlangıç verilerini sıfırdan kursun.
   - Ayrıca `EfCoreSyntheticPortfolioGenerator.cs` ile istenirse portföyü 2.000+ müşteriye (Persuadable, Free-Rider, Dormant/Uyuyan Kart, Ayrıcalıklar Dünyası segmentleri ve 12 aylık enflasyonlu işlem geçmişi) genişleten EF Core toplu veri üreticisini ekle.
4. `< 500 ms` (KPI-5) performans hedefi için `CompiledQueries/Odea028CompiledQueries.cs` içinde `EF.CompileAsyncQuery` ile önceden derlenmiş yüksek hızlı EF Core sorgularını (`GetMerchantCacheByKeyAsync`, `GetActiveTrackerWithAssignmentAsync`, `GetRecentMerchantTransactionsForSplitCheckAsync`) oluştur.
5. `Odea028.Tests` içinde `MSTest` ve `Moq` ile hem birim testlerini hem de SQL Server şema/veri doğrulama testlerini yaz ve çalıştır.
```

---

## 🧩 PROMPT 3 (3. Proje): `Odea028.PosEnrichment` — 3 Katmanlı POS İşlem Zenginleştirme Motoru (İP-2)

*(2. Projeyi inceleyip onayladıktan sonra, 3. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 3. Aşamasına (`src/Odea028.PosEnrichment` — 3 Katmanlı Hibrit NLP/ML POS İşlem Zenginleştirme Motoru) geçiyoruz.

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0`, `Entity Framework Core 10.0` ve `Microsoft.ML` (`FeaturizeText` + `LightGbmMulticlass`) kullanılacaktır.
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce 3 katmanlı POS sınıflandırma mimarisini, regex kurallarını, ML.NET eğitim boru hattını, EF Core `MERCHANT_MASTER_CACHE` entegrasyonunu ve `KPI-1 (Macro F1-Score => %92,0)` MSTest + Moq test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (3. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.PosEnrichment` projesini oluştur.
2. ISO 8583 `DE-18 (MCC)` ve `DE-43 (Üye İşyeri Adı/Lokasyonu)` alanlarındaki ödeme kuruluşu (`PF`) ve hatalı POS tanımı kirliliğini çözen 3 katmanlı `PosEnrichmentService.cs` motorunu kodla:
   - **Önbellek Katmanı:** Bellekte `ConcurrentDictionary` ve SQL Server'da EF Core `MerchantMasterCaches` tablosu üzerinden `< 1 ms` içinde `O(1)` sorgulama.
   - **Katman 1 — Ulusal Zincir ve Yasaklı MCC Sözlüğü (`DeterministicBrandDictionary.cs`):** `FrozenDictionary` ile ulusal zincirleri (`MİGROS`, `CARREFOURSA`, `BİM`, `A101`, `SHELL`, `OPET`, `PETROL OFİSİ`, `THY`, `PEGASUS`, `LC WAIKIKI`, `BOYNER`, `TEKNOSA`, `MEDIAMARKT`, `STARBUCKS` vb.) ve nakit benzeri/yasaklı MCC kodlarını (`6011`, `6012`, `6051`, `7995`, `4829`) anında sınıflandır.
   - **Katman 2 — Ödeme Kuruluşu Alt-İşyeri Ayrıştırıcı (`PaymentFacilitatorRegexParser.cs`):** C# `[GeneratedRegex]` ile `IYZICO*`, `PARAM*`, `PAYTR*`, `SIPAY*`, `MOKA*`, `ODEAL*`, `CRAFTGATE*`, `PAYCELL*` ön eklerini, şehir/terminal eklerini temizleyip alt markayı (`TRENDYOL`, `GETİR`, `HEPSİBURADA`, `ZARA`, `YEMEKSEPETİ` vb.) ve ödeme kuruluşu adını çıkar.
   - **Katman 3 — ML.NET Yerel Semantik Metin Sınıflandırıcı (`LocalMlMerchantClassifier.cs`):** İlk kez karşılaşılan uzun kuyruk (long-tail) POS işyeri metinlerini karakter/kelime n-gram'ları ve MCC kodu üzerinden `LightGbmMulticlass` ile 9 ana sektöre sınıflandır ve EF Core ile `MERCHANT_MASTER_CACHE` tablosuna kaydet.
3. `Odea028.Tests` içinde `MSTest` ve `Moq` (EF Core repository/context mock'laması dahil) kullanarak:
   - Altın standart (ground-truth) POS test veri setinde **KPI-1: Macro F1-Score `=> %92,0`** olduğunu,
   - Ödeme kuruluşu (`IYZICO*`, `PARAM*`, `PAYTR*`) ayrıştırmasının ve EF Core önbellek kaydının hatasız çalıştığını doğrulayan testleri yaz ve çalıştır.
```

---

## 🧩 PROMPT 4 (4. Proje): `Odea028.CausalAi` — R-Learner Uplift, Bandit, XAI, HDBSCAN ve CUPED Motoru (İP-3A & İP-4)

*(3. Projeyi inceleyip onayladıktan sonra, 4. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 4. Aşamasına (`src/Odea028.CausalAi` — Enflasyon-Düzeltmeli Feature Store, Robinson R-Learner Nedensel Uplift, Contextual Bandit, XAI, HDBSCAN, PSI Drift ve CUPED Deney Motoru) geçiyoruz.

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0`, `Entity Framework Core 10.0`, `Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.Recommender` ve `MathNet.Numerics` kullanılacaktır.
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce R-Learner matematiksel formüllerini, Empirik Bayesyen Küçültme (Shrinkage) hesabını, Thompson Sampling Bandit yapısını, ML.NET `CalculateFeatureContributions` Türkçe XAI gerekçe üretimini, HDBSCAN/PSI/CUPED algoritmalarını ve MSTest + Moq test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (4. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.CausalAi` projesini oluştur.
2. **Enflasyon-Düzeltmeli Feature Store (`EfCoreCausalFeatureStore.cs`):** EF Core üzerinden `vw_CustomerCausalFeatures` ve `INFLATION_INDEX_MONTHLY` verilerini çekerek nominal TL yerine reel harcama endeksi, portföy harcama kuantili (`PortfolioRealSpendQuantile`), sektör çeşitliliği, boş limit oranı ve MIS kârlılık vektörünü hazırlayan servisi yaz.
3. **İki Aşamalı Robinson Ayrıştırmalı R-Learner ve Empirik Bayesyen Küçültme (`RobinsonRLearnerUpliftEngine.cs`):**
   - **1. Aşama:** `MLContext.Regression.Trainers.LightGbm` ile kampanyasız **Beklenen Organik Harcamayı** ($\hat{\mu}_0(X)$) ve müdahale olasılığını ($\hat{e}(X) \in [0.05, 0.95]$) tahmin et.
   - **2. Aşama:** Harcama artığı $Y_{res} = Y - \hat{\mu}_0(X)$ ve müdahale artığı $W_{res} = W - \hat{e}(X)$ üzerinden sözde-hedef $\tilde{\tau} = Y_{res} / W_{res}$ ve ağırlık $w = W_{res}^2$ ile ikinci `LightGbm` modelini eğit. Düşük örneklem varyansını bastırmak için `MathNet.Numerics` ile **Empirik Bayesyen Küçültme (Shrinkage)** uygula ve **Bireysel Kampanya Etkisi (ITE / Uplift)** skorunu üret.
   - **İnaktif (Uyuyan) Kart Uyandırma Modeli (`DormantCardReactivationModel.cs`):** Son 90+ gün işlemi olmayan kartlar için 60 günlük kalıcı aktifleşme olasılığını (`KPI-4: => %6,5`) hesapla.
4. **Sektörel Öneri (`MatrixFactorizationSectorRecommender.cs`) ve Contextual Bandit (`ThompsonSamplingBanditService.cs`):**
   - `Microsoft.ML.Recommender` ile müşterinin potansiyel harcama yapacağı yeni sektörü tahmin et.
   - `MathNet.Numerics.Distributions.Beta` ile hedef/ödül paketlerinde keşif-kullanım (exploration-exploitation) yapan ve müşterinin Odea Mobil'deki Puan vs. Nakit İade seçiminden öğrenen (Discrete Choice) servisi yaz.
5. **Açıklanabilir Yapay Zekâ (`XaiTurkishExplanationService.cs`), HDBSCAN Kümeleme (`BehavioralHdbscanClusterer.cs`), PSI Drift (`InflationPsiDriftMonitor.cs`) ve CUPED (`CupedExperimentEvaluator.cs`):**
   - ML.NET `CalculateFeatureContributions` ile her müşteri kararı için en etkili 3 özniteliği çıkarıp Türkçe gerekçe metnine çevir (`KPI-8: %100 Gerekçe Üretimi`).
   - Yoğunluk tabanlı HDBSCAN davranışsal mikro-küme keşfini ve aylık `PSI < 0,15` (`KPI-7`) enflasyon kayması kontrolünü kodla.
   - Deney öncesi harcamayı ortak değişken olarak kullanan **CUPED** ($\hat{Y}_{CUPED} = Y_{post} - \theta (X_{pre} - \bar{X}_{pre})$) ile `(İlave Harcama * Marj - Ödül)` net ekonomik katkısını ve `Incremental ROI => 1,35x` (`KPI-3`) ölçümünü kodla.
6. `Odea028.Tests` içinde `MSTest` ve `Moq` kullanarak Free-Rider ayrımını (`KPI-2: <= %25`), Uyuyan Kart dönüşümünü (`KPI-4`), PSI Drift (`KPI-7`), CUPED ROI (`KPI-3`) ve XAI Türkçe gerekçe üretimini doğrulayan testleri yaz ve çalıştır.
```

---

## 🧩 PROMPT 5 (5. Proje): `Odea028.Optimization` — `Google.OrTools` MILP Bütçe/Kârlılık/Adillik Optimizatörü (İP-3B)

*(4. Projeyi inceleyip onayladıktan sonra, 5. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 5. Aşamasına (`src/Odea028.Optimization` — `Google.OrTools` Karma Tamsayılı Doğrusal Programlama / MILP Bütçe, Kârlılık ve Adillik Optimizatörü) geçiyoruz.

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0`, `Entity Framework Core 10.0` ve `Google.OrTools` (`Google.OrTools.LinearSolver` - `SCIP` / `CBC`) kullanılacaktır.
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce 20 Mikro-Kova yapısını, MILP amaç fonksiyonunu, 5 matematiksel kısıt denklemini (Monotonluk/Adillik kısıtı dahil), 3 eşdeğer teklif üretim mantığını, Stratified RCT Hold-out ayırıcısını ve MSTest + Moq test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (5. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.Optimization` projesini oluştur ve `Google.OrTools` paketini ekle.
2. **20 Standart Mikro-Kova Üretici (`MicroBucketCatalogService.cs`):** Operasyonel yönetilebilirlik için harcama hedeflerini ve ödül seviyelerini 20 standart mikro-kovaya (`MicroBucketId: 1..20` — İnaktif Uyandırma, Sektörel Çapraz Satış ve Harcama Artışı kovaları) yapılandır.
3. **`Google.OrTools` MILP Portföy Optimizatörü (`MilpPortfolioOptimizer.cs`):**
   - Karar değişkeni $x_{i,k} \in \{0,1\}$ (Müşteri $i$'ye mikro-kova $k$ atanması).
   - **Amaç Fonksiyonu:** Toplam Net İlave Ekonomik Katkıyı $\sum_i \sum_k \left( (\Delta\text{Uplift}_{i,k} \times \text{Marj}) - \text{Ödül}_{i,k} \right) x_{i,k}$ maksimize et.
   - **Kısıt 1 (Tekil Atama ve Free-Rider Koruması):** Her müşteriye en fazla 1 teklif atanır ($\sum_k x_{i,k} \le 1$). Net ekonomik katkısı $\le 0$ olan veya "Free-Rider" olan müşterilere bütçe harcanmaz ($x_{i,k} = 0$).
   - **Kısıt 2 (Toplam Kampanya Bütçesi):** $\sum_i \sum_k \text{Ödül}_{i,k} \cdot x_{i,k} \le B_{\text{toplam}}$.
   - **Kısıt 3 (Boş Kart Limiti ve Tavan Ödül Kısıtı):** Atanan harcama hedefi müşterinin kullanılabilir boş kredi kartı limitinin %80'ini aşamaz ($\text{Hedef}_{i,k} \le 0.80 \times \text{BoşLimit}_i$) ve ödül müşteri başı tavanı aşamaz.
   - **Kısıt 4 (MIS Kârlılık, KVKK/İYS İzni ve Ayrıcalıklar Dünyası Segment Uyumu):** KVKK/İYS izni olmayan veya MIS kârlılığı eşik altında kalanlara ödül atanmaz; Ayrıcalıklar Dünyası Altın/Özel segment taban avantaj kuralları korunur.
   - **Kısıt 5 (Algoritmik Adillik / Monotonluk Kısıtı):** Aynı segmentte daha yüksek harcama hedefi atanan bir müşterinin mutlak ödül tutarı, daha düşük hedef atanan müşteriden kesinlikle düşük olamaz ($\text{Hedef}_a > \text{Hedef}_b \implies \text{Ödül}_a \ge \text{Ödül}_b$).
4. **Eşdeğer Kârlılıklı 3 Teklif Seçeneği (`EquivalentOfferBundleBuilder.cs`) ve Stratified RCT Ayırıcı (`StratifiedRctHoldoutService.cs`):**
   - Odea Mobil'de müşterinin kendi avantajını seçebilmesi için banka açısından beklenen net katkısı birbirine eşdeğer 3 teklif seçeneğini (`AlternativeOffersJson`: 1. Puan odaklı, 2. Nakit İade odaklı, 3. Sektörel avantaj) üret.
   - Portföyü harcama dilimi ve segment bazında tabakalayarak `%90 Müdahale (Treatment)` ve `%10 Kontrol (Control_Holdout)` grubuna ayırıp EF Core ile `CUSTOMER_OFFER_ASSIGNMENT` ve `OFFER_PROGRESS_TRACKER` tablolarına kaydet.
5. `Odea028.Tests` içinde `MSTest` ve `Moq` kullanarak bütçe kısıtının aşılmadığını, Free-Rider oranının `<= %25` olduğunu (`KPI-2`), Monotonluk/Adillik kısıtının `%0` ihlalle çalıştığını (`KPI-8`) ve optimizasyon süresinin (`KPI-6`) hedef sınırlar içinde kaldığını doğrulayan testleri yaz ve çalıştır.
```

---

## 🧩 PROMPT 6 (6. Proje): `Odea028.Application` — Gerçek Zamanlı Sayaç (`< 500 ms`), Anti-Gaming ve Clawback Orkestrasyonu (İP-3C)

*(5. Projeyi inceleyip onayladıktan sonra, 6. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 6. Aşamasına (`src/Odea028.Application` — EF Core 10.0 Gerçek Zamanlı Provizyon Sayaç Motoru, Anti-Gaming Filtreleri, İade Clawback ve Kampanya Orkestrasyon Katmanı) geçiyoruz.

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0` ve `Entity Framework Core 10.0` (`EF.CompileAsyncQuery` ve `RowVersion` Optimistic Concurrency) kullanılacaktır (Dapper kullanılmayacaktır).
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce gerçek zamanlı ISO 8583 `0100/0200` provizyon işleme akışını, Anti-Gaming (nakit benzeri MCC ve 30 dk bölünmüş işlem) kurallarını, iade/iptal durumunda ödül geri alma (`Clawback`) mekanizmasını, `Kill-Switch` akışını ve MSTest + Moq test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (6. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.Application` projesini oluştur ve `Core`, `Infrastructure`, `PosEnrichment`, `CausalAi`, `Optimization` projelerini orkestre et.
2. **Gerçek Zamanlı Provizyon ve Sayaç Servisi (`RealtimeOfferTrackerService.cs`):**
   - Kart provizyonu geldiğinde `< 500 ms` (`KPI-5`) altında EF Core derlenmiş sorgularıyla çalışacak uçtan uca akışı kodla:
     - **Adım 1:** `IPosEnrichmentService` ile `DE-18 MCC` ve `DE-43 İşyeri Adı` üzerinden gerçek marka ve sektörü çözümle.
     - **Adım 2 (Kill-Switch Kontrolü):** Kampanyada `KillSwitchTriggered == true` ise sayacı ilerletme.
     - **Adım 3 (Anti-Gaming 1 — Nakit Benzeri / Yasaklı MCC):** İşlem nakit çekim, döviz, altın/kuyum, bahis veya para transferi MCC kodundaysa (`6011`, `6012`, `6051`, `7995`, `4829`) `IsExcludedByAntiGaming = true` olarak kaydet ve sayaca ekleme.
     - **Adım 4 (Anti-Gaming 2 — 30 Dk Bölünmüş İşlem / Split-Transaction Engeli):** Aynı müşteri aynı üye işyerinde (veya aynı temizlenmiş markada) son 30 dakika içinde işlem yapmışsa bölünmüş işlem suistimali olarak işaretle, `ExcludedTxCount` değerini artır ve kampanya sayacını artırma.
     - **Adım 5 (Atomik Sayaç Güncelleme & Ödül Hak Edişi):** İşlem geçerliyse `OFFER_PROGRESS_TRACKER` tablosunda `CurrentQualifiedSpend`, `RemainingAmount`, `ProgressPercentage` alanlarını güncelle. `CurrentQualifiedSpend => TargetAmount` olduğu anda `IsCompleted = true` yap ve `REWARD_FULFILLMENT_QUEUE` tablosuna `PendingSettlement` statüsünde ödül kaydı aç.
3. **İptal / İade ve Ödül Geri Alma (`Clawback`) Servisi (`ProcessReversalOrRefundAsync`):**
   - İade/iptal işlemi geldiğinde sayacı düşür; eğer toplam geçerli harcama `TargetAmount` altına inerse `IsCompleted = false` yap ve `REWARD_FULFILLMENT_QUEUE` kaydını `Status = "ClawedBack"` (`ClawbackReason = "İade kaynaklı hedef altına düşüş"`) olarak güncelle.
4. **Müşteri Tercih Seçimi (`SelectAlternativeOfferAsync`) ve Yönetim (`TriggerKillSwitchAsync`) Servisi:**
   - Müşteri Odea Mobil'de 3 eşdeğer tekliften birini seçtiğinde hedef/ödül tipini güncelleyen ve Contextual Bandit tercih modelini besleyen metodu yaz.
5. `Odea028.Tests` içinde `MSTest` ve `Moq` kullanarak `< 500 ms` gecikme (`KPI-5`), 30 dk bölünmüş işlem engeli, yasaklı MCC filtresi, iade `Clawback` mekanizması ve `Kill-Switch` senaryolarını `%100` doğrulayan testleri yaz ve çalıştır.
```

---

## 🧩 PROMPT 7 (7. Proje): `Odea028.Web` — `ASP.NET Core 10.0` API & 3'ü 1 Arada Blazor Komuta Merkezi (İP-5 & İP-6)

*(6. Projeyi inceleyip onayladıktan sonra, son aşama olan 7. Proje için aşağıdaki promptu bana yapıştırın.)*

```text
ODEA-028 projesinin 7. ve Son Aşamasına (`src/Odea028.Web` — `ASP.NET Core 10.0` REST API ve 3'ü 1 Arada Blazor Interactive Server Görsel Komuta Merkezi) geçiyoruz.

KESİN TEKNOLOJİ VE ÇALIŞMA KURALLARI:
1. `.NET 10.0` (`ASP.NET Core 10.0` Web API + Blazor Interactive Server) ve `Entity Framework Core 10.0` kullanılacaktır.
2. Testlerde yalnızca `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) ve `Moq` kullanılacaktır.
3. DİKKAT - ÖNCE PLAN SUN: Kod yazmadan önce REST API Controller uç noktalarını, 3 Sekmeli Blazor ekran tasarımını (Odea Mobil Simülatörü, Back-Office Yönetim/Kill-Switch Paneli, Çağrı Merkezi Ekranı) ve MSTest + Moq Controller/UI servis test planını sun; onayımı bekle. Onay sonrası kodla, test et ve incelemem için dur.

BU AŞAMADA (7. PROJEDE) İNŞA EDİLECEKLER:
1. `src/Odea028.Web` (`net10.0`) projesini oluştur, `Program.cs` içinde EF Core 10.0 `Odea028DbContext` ve tüm servislerin Dependency Injection kayıtlarını yap; uygulama ayağa kalkarken veritabanı yoksa `SqlDatabaseBootstrapper` ile otomatik hazırla.
2. **REST API Controller'ları (`Controllers/CampaignEngineController.cs`):**
   - `POST /api/v1/transactions/authorize` (Canlı ISO 8583 kart provizyonu geçirir, POS zenginleştirme + Anti-Gaming + Sayaç yanıtını ve `LatencyMs` değerini döner)
   - `POST /api/v1/transactions/reversal` (İptal/iade geçirir ve Clawback durumunu döner)
   - `POST /api/v1/campaigns/{campaignId}/optimize` (R-Learner Uplift + Google.OrTools MILP optimizasyonunu çalıştırır)
   - `POST /api/v1/offers/{assignmentId}/select-option/{optionIndex}` (Müşterinin 3 eşdeğer tekliften seçimini kaydeder)
   - `POST /api/v1/campaigns/{campaignId}/kill-switch` (Acil durdurma tetikler)
3. **3'ü 1 Arada İnteraktif Blazor Komuta Merkezi (`Components/Pages/Home.razor`):**
   - **SEKME 1: 📱 Odea Mobil Müşteri Telefon Simülatörü**
     - Müşteri seçici (Ahmet Yılmaz - Persuadable Altın, Zeynep Kaya - Nakit İade Seven, Mehmet Demir - Free-Rider, Elif Şahin - Uyuyan Kart).
     - Mobil telefon ekranı içinde: Kişiye Özel Teklif Kartı, **XAI Türkçe Karar Gerekçesi**, **"Avantajını Kendin Seç" (3 Eşdeğer Teklif Seçim Butonları)** ve **Canlı İlerleme Çubuğu (`Progress Bar`)**.
     - **Canlı POS İşlem & Anti-Gaming Test Paneli:** Tek tıkla `IYZICO*TRENDYOL.COM` (Geçerli), `PARAM*GETIR` (30 dk içinde 2. Bölünmüş İşlem - Anti-Gaming), `ALTINBAS KUYUMCULUK MCC 6051` (Yasaklı MCC - Anti-Gaming) veya `İade / Refund (Clawback)` işlemi geçirip sayacın `< 500 ms` içinde canlı değişimini ekranda göster.
   - **SEKME 2: 🎛️ Back-Office Yönetim, 8 KPI, CUPED ve Kill-Switch Paneli**
     - 8 KPI Canlı Durum Kartları (`KPI-1`..`KPI-8` hedef ve gerçekleşen değerler).
     - **"🚀 R-Learner + MILP Portföy Optimizasyonunu Çalıştır"** butonu ve 20 Mikro-Kova atama özeti.
     - **CUPED Net Ekonomik Katkı Raporu**, **Aylık PSI Drift İzleme**, **HDBSCAN Davranışsal Küme Özeti** ve **"🛑 ACİL DURDUR (KILL-SWITCH)"** butonu.
   - **SEKME 3: 🎧 Çağrı Merkezi Müşteri Gerekçe & Denetim Ekranı**
     - Müşteri temsilcisinin müşteriye atanan teklifi, XAI öznitelik katkı ağırlıklarını, sayaca dahil edilen işlemleri ve Anti-Gaming nedeniyle reddedilen işlemleri gerekçeleriyle gördüğü ekran.
4. `Odea028.Tests` içinde `MSTest` ve `Moq` kullanarak `CampaignEngineController` uç noktalarını ve uçtan uca 8 KPI doğrulamasını test et; tüm çözümde `dotnet build` ve `dotnet test` komutlarının `%100` başarıyla geçtiğini doğrula.
```
