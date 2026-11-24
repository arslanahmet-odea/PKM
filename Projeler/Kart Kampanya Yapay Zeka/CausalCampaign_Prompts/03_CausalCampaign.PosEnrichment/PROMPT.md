# PROJE 3 PROMPTU: `CausalCampaign.PosEnrichment` (3 Katmanlı POS NLP/ML İşlem Zenginleştirme Motoru)

*(2. Projeyi inceleyip onayladıktan sonra, 3. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0, Doğal Dil İşleme (ML.NET) ve Microsoft SQL Server konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **3. Aşamasına (`src/CausalCampaign.PosEnrichment` — 3 Katmanlı Hibrit NLP/ML POS İşlem Zenginleştirme Motoru)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını koru (`CausalCampaign.PosEnrichment`, `CausalCampaign.Domain` içerisindeki `IPosEnrichmentService` arayüzünü uygular).
   - Veritabanı önbellek erişimi yalnızca `ICausalCampaignDbContext` (EF Core 10.0) üzerinden yapılacaktır.
   - Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal (3 katmanlı sınıflandırma için **Chain of Responsibility / Strategy** desenini uygula).
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
   - Yüksek performans için C# `.NET 10.0` `FrozenDictionary`, `SearchValues<char>` ve derleme zamanı kaynak üreticili `[GeneratedRegex]` kullan.

3. **Hata Yönetimi ve Güvenlik:**
   - Bozuk, boş veya beklenmeyen ISO 8583 `DE-18 (MCC)` ve `DE-43 (Üye İşyeri Adı/Lokasyonu)` girdilerini savunmacı programlama (guard clauses) ve `Result<PosEnrichmentResultDto>` ile yönet.
   - Model dosya yollarını veya eşik değerlerini kod içine gömme; `IOptions<PosEnrichmentOptions>` ile yapılandır.

4. **Test ve Dokümantasyon:**
   - Testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - **KPI-1 Hedefi:** Altın standart (ground-truth) POS test seti üzerinde **Macro F1-Score `=> %92,0`** başarısını birim testte matematiksel olarak hesapla ve doğrula.
   - Tüm sınıflara ve karmaşık metin ayrıştırma/ML fonksiyonlarına **XML Documentation (`/// <summary>`)** ekle.

---

### BU AŞAMADA (`CausalCampaign.PosEnrichment`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.PosEnrichment` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve `CausalCampaign.Domain` referansını bağla.
   - Paketler: `Microsoft.ML`, `Microsoft.ML.LightGbm`.
2. **Önbellek Yöneticisi (`Caching/PosMerchantCacheManager.cs`):**
   - L1 Bellek İçi Önbellek (`ConcurrentDictionary<string, MerchantMasterCache>`) + L2 SQL Server EF Core `MerchantMasterCaches` tablosu üzerinden `< 1 ms` `O(1)` okuma ve yeni çözümlenen işyerlerini kaydetme.
3. **Katman 1 — Deterministik Ulusal Zincir ve Yasaklı MCC Sözlüğü (`Layers/DeterministicBrandDictionaryLayer.cs`):**
   - `FrozenDictionary` tabanlı ulusal zincir sözlüğü (`MİGROS`, `CARREFOURSA`, `BİM`, `A101`, `ŞOK`, `MACROCENTER`, `SHELL`, `OPET`, `PETROL OFİSİ`, `BP`, `THY`, `PEGASUS`, `LC WAIKIKI`, `DEFACTO`, `MAVİ`, `BOYNER`, `ZARA`, `TEKNOSA`, `MEDIAMARKT`, `VATAN BİLGİSAYAR`, `STARBUCKS`, `KAHVE DÜNYASI`, `ECZANE` vb.).
   - Nakit benzeri / kampanya dışı yasaklı MCC (`6011`, `6012`, `6051`, `7995`, `4829`) ve anahtar kelime (`KUYUMCU`, `ALTIN`, `DÖVİZ`, `BAHİS`, `CASINO`, `KRİPTO`) filtresi.
4. **Katman 2 — Ödeme Kuruluşu (PF / Aggregator) Alt-İşyeri Ayrıştırıcı (`Layers/PaymentFacilitatorRegexLayer.cs`):**
   - C# `[GeneratedRegex]` ile `IYZICO*`, `PARAM*`, `PAYTR*`, `SIPAY*`, `MOKA*`, `ODEAL*`, `CRAFTGATE*`, `PAYCELL*`, `PAPARA*` ön eklerini, şehir adlarını (`ISTANBUL`, `ANKARA`, `TR`) ve terminal numaralarını temizleyip gerçek alt markayı (`TRENDYOL`, `HEPSİBURADA`, `GETİR`, `YEMEKSEPETİ`, `AMAZON`, `N11`, `ÇİÇEKSEPETİ`, `MARTI` vb.) ve aracı ödeme kuruluşu adını (`SubMerchantProvider`) çıkaran katman.
5. **Katman 3 — ML.NET Yerel Semantik Metin Sınıflandırıcı (`Layers/LocalMlMerchantClassifierLayer.cs`):**
   - İlk iki katmanda eşleşmeyen uzun kuyruk (long-tail) POS işyeri metinlerini (`BEREKET UNLU MAMULLERI`, `ANADOLU ET GALERISI`, `MODA BUTIK TEKSTIL`, `GUNES ELEKTRIK ELEKTRONIK`, `LEZZET DURAGI KEBAP` vb.) karakter + kelime n-gram'ları (`FeaturizeText`) ve `RawMcc_DE18` üzerinden `LightGbmMulticlass` ile 9 standart sektöre (`Market & Gıda`, `Akaryakıt`, `E-Ticaret & Pazaryeri`, `Giyim & Tekstil`, `Elektronik`, `Restoran & Kafe`, `Seyahat & Konaklama`, `Sağlık & Kozmetik`, `Nakit Benzeri / Yasaklı`) sınıflandıran ve güven skorunu (`ConfidenceScore`) hesaplayan katman.
6. **Orkestratör Servis (`Services/PosEnrichmentService.cs`):**
   - `IPosEnrichmentService` arayüzünü uygulayan, L1/L2 Cache -> Katman 1 -> Katman 2 -> Katman 3 zincirini çalıştıran ve Macro F1-Score değerlendirme metodunu (`EvaluateMacroF1Score`) içeren ana servis.
7. **Birim Testler (`tests/CausalCampaign.UnitTests/PosEnrichmentTests/`):**
   - `MSTest` ve `Moq` (`Mock<ICausalCampaignDbContext>`) kullanarak:
     - Ulusal zincir, ödeme kuruluşu (`IYZICO*`, `PARAM*`, `PAYTR*`, `SIPAY*`), uzun kuyruk ML sınıflandırma ve yasaklı MCC testleri,
     - **KPI-1 Doğrulama Testi:** 100+ örnekli zorlu POS test veri setinde **Macro F1-Score `=> 0.92` (`%92,0`)** ve önbellek yanıt süresinin `< 1 ms` olduğunun doğrulanması.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.PosEnrichment` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `[GeneratedRegex]`, `FrozenDictionary`, `Microsoft.ML` (`FeaturizeText` + `LightGbmMulticlass`), `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** 3 katmanlı Chain of Responsibility yapısı, L1/L2 Cache stratejisi ve `KPI-1 (Macro F1 => %92)` garantisi.

Sonunda **"3. Aşama (`CausalCampaign.PosEnrichment`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile doğrulayıp incelemem için dur.
```
