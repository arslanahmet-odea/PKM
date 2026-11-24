# PROJE 5 PROMPTU: `CausalCampaign.Optimization` (`Google.OrTools` MILP Bütçe, Kârlılık ve Adillik Optimizatörü)

*(4. Projeyi inceleyip onayladıktan sonra, 5. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0, Yöneylem Araştırması / Matematiksel Optimizasyon (`Google.OrTools`) ve Microsoft SQL Server konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **5. Aşamasına (`src/CausalCampaign.Optimization` — `Google.OrTools` Karma Tamsayılı Doğrusal Programlama / MILP Bütçe, Kârlılık ve Adillik Optimizatörü)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını koru (`CausalCampaign.Optimization`, `CausalCampaign.Domain` içindeki `IMilpPortfolioOptimizer` arayüzünü uygular).
   - Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
   - Çözücü (Solver) bellek sızıntılarını önlemek için `Google.OrTools.LinearSolver.Solver` nesnesini `using` bloğu içinde deterministik olarak yönet.

3. **Hata Yönetimi ve Güvenlik:**
   - MILP çözücüsünün `OPTIMAL`, `FEASIBLE`, `INFEASIBLE` veya `ABNORMAL` durumlarını açıkça kontrol et ve `Result<PortfolioOptimizationResultDto>` ile döndür.
   - Bütçe, tavan ödül, boş limit çarpanı (`0.80m`) ve hold-out oranı (`0.10m`) parametrelerini `IOptions<OptimizationOptions>` ile yapılandır.

4. **Test ve Dokümantasyon:**
   - Testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - Amaç fonksiyonu ve 5 matematiksel kısıt denklemi için detaylı **XML Documentation (`/// <summary>`)** açıklamaları yaz.

---

### BU AŞAMADA (`CausalCampaign.Optimization`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.Optimization` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve `CausalCampaign.Domain` referansını bağla.
   - Paket: `Google.OrTools`.
2. **20 Standart Mikro-Kova Kataloğu (`Buckets/MicroBucketCatalogService.cs`):**
   - Operasyonel karmaşayı önlemek için harcama hedeflerini ve ödül tutarlarını 20 standart mikro-kovaya (`MicroBucketId: 1..20`) yapılandır:
     - `Kova 1..4`: İnaktif (Uyuyan) Kart Uyandırma Kovaları (`2.500 TL` – `7.500 TL` hedef)
     - `Kova 5..10`: Sektörel Çapraz Harcama Kovaları (`10.000 TL` – `25.000 TL` hedef)
     - `Kova 11..20`: Toplam Harcama Artışı (Upsell) Kovaları (`15.000 TL` – `100.000 TL` hedef)
   - Kova kataloğunda **Monotonluk (Algoritmik Adillik)** kuralını yapısal olarak zorunlu kıl: Aynı kampanya grubunda `Hedef_a > Hedef_b => Ödül_a => Ödül_b`.
3. **`Google.OrTools` MILP Portföy Optimizatörü (`Solvers/MilpPortfolioOptimizer.cs`):**
   - `Solver.CreateSolver("SCIP")` (veya `"CBC_MIXED_INTEGER_PROGRAMMING"`) kullanarak karar değişkeni $x_{i,k} \in \{0, 1\}$ (Müşteri $i$'ye mikro-kova $k$ atanması) üzerinden modeli kur:
   - **Amaç Fonksiyonu:** Toplam Net İlave Ekonomik Katkıyı maksimize et:
     $$\max \sum_{i} \sum_{k} \left( (\Delta\text{Uplift}_{i,k} \times \text{Marj}) - \text{Ödül}_{i,k} + w_{MIS} \cdot \text{MISKatkı}_i \right) x_{i,k}$$
   - **Kısıt 1 (Tekil Atama & Free-Rider Elemesi):** Her müşteriye en fazla 1 mikro-kova atanabilir ($\sum_k x_{i,k} <= 1$). Net ekonomik katkısı $<= 0$ olan veya `FreeRider_SureThing` olarak tespit edilen müşterilere bütçe harcanmaz ($x_{i,k} = 0$, `KPI-2: Free-Rider <= %25`).
   - **Kısıt 2 (Toplam Kampanya Bütçesi Kısıtı):** $\sum_i \sum_k \text{Ödül}_{i,k} \cdot x_{i,k} <= \text{TotalBudget}$.
   - **Kısıt 3 (Boş Kart Limiti ve Tavan Ödül Kısıtı):** Atanan harcama hedefi müşterinin kullanılabilir boş kredi kartı limitinin %80'ini aşamaz ($\text{Hedef}_{i,k} <= 0.80 \times \text{AvailableCreditLimit}_i$) ve ödül `MaxRewardPerCustomer` tavanını aşamaz.
   - **Kısıt 4 (MIS Kârlılık, Pazarlama İzni ve Segment Uyumu):** `HasMarketingConsent == false` olan veya `MisMonthlyNetProfit < MinMisProfitThreshold` olan müşterilere teklif atanmaz; `Gold` ve `Private` segment taban ödül kuralları korunur.
   - **Kısıt 5 (Algoritmik Adillik / Monotonluk Kısıtı - `KPI-8`):** Aynı segmentte daha yüksek harcama hedefi (`AssignedTargetAmount`) atanan bir müşterinin mutlak ödül tutarı (`AssignedRewardAmount`), daha düşük harcama hedefi atanan bir müşteriden kesinlikle düşük olamaz (`Hedef_i > Hedef_j => Ödül_i => Ödül_j`).
4. **Eşdeğer Kârlılıklı 3 Teklif Paketi Üretici (`Bundles/EquivalentOfferBundleBuilder.cs`):**
   - Müşterinin mobil uygulamada kendi avantajını seçebilmesi (Active Preference Learning) için banka açısından beklenen net katkısı birbirine eşdeğer 3 teklif seçeneğini (`AlternativeOffersJson`: 1. Puan odaklı, 2. Nakit İade odaklı, 3. Sektörel avantaj) üret.
5. **Tabakalı Rastgeleleştirilmiş Kontrol Grubu Ayırıcı (`Experiment/StratifiedRctHoldoutSplitter.cs`):**
   - Uygun müşterileri harcama dilimi (`SpendDecile`) ve `PrestigeSegment` bazında tabakalayarak `%90 Treatment` ve `%10 Control_Holdout` grubuna ayır.
6. **Birim Testler (`tests/CausalCampaign.UnitTests/OptimizationTests/`):**
   - `MSTest` ve `Moq` kullanarak:
     - Toplam bütçenin kuruşu kuruşuna korunduğunun,
     - Boş limiti düşük müşteriye (`AvailableCreditLimit = 4.500 TL`) limit üstü hedef atanmadığının,
     - Free-Rider oranının `<= %25` kaldığının (`KPI-2`),
     - **Monotonluk / Algoritmik Adillik kısıtının `%0` ihlalle sağlandığının (`KPI-8`)**,
     - Çözüm süresinin `< 15 dakika` (test portföyünde `< 2 saniye`, `KPI-6`) olduğunun doğrulanması.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.Optimization` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `Google.OrTools.LinearSolver` (`SCIP`/`CBC`), `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** 20 Mikro-Kova tasarımı, 5 kısıtlı MILP matematiksel modeli ve Monotonluk (Adillik) kısıtının sıfır ihlal garantisi.

Sonunda **"5. Aşama (`CausalCampaign.Optimization`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile doğrulayıp incelemem için dur.
```
