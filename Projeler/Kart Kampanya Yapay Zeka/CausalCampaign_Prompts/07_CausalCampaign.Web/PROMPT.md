# PROJE 7 PROMPTU: `CausalCampaign.Web` (`ASP.NET Core 10.0` Web API, Global Error Handling & 3'ü 1 Arada Blazor Komuta Merkezi)

*(6. Projeyi inceleyip onayladıktan sonra, son aşama olan 7. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen `ASP.NET Core 10.0`, `Blazor Interactive Server`, `Entity Framework Core 10.0` ve Kurumsal Güvenlik/Hata Yönetimi konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **7. ve Son Aşamasına (`src/CausalCampaign.Web` — `ASP.NET Core 10.0` REST API, Merkezi Hata Yönetimi ve 3'ü 1 Arada Blazor Görsel Komuta Merkezi)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını tamamla (`CausalCampaign.Web` sunum ve kompozisyon köküdür / Composition Root).
   - Controller'lar ve Blazor bileşenleri içinde hiçbir iş mantığı veya doğrudan SQL/DbContext sorgusu bulunmamalıdır; tüm işlemler `CausalCampaign.Application` servisleri üzerinden yürütülmelidir.
   - Veritabanı adı `CausalCampaignDb`'dir. Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

3. **Hata Yönetimi ve Güvenlik:**
   - **Merkezi Hata Yönetimi (Global Error Handling):** `ASP.NET Core 10.0` `IExceptionHandler` (`GlobalExceptionHandler.cs`) ve RFC 7807 `ProblemDetails` standardını kur. Beklenmeyen tüm hataları yakala, hassas stack-trace bilgilerini dışarı sızdırmadan standart `ProblemDetails` JSON formatında ve anlamlı Türkçe mesajlarla döndür.
   - **Yapılandırma Güvenliği:** Connection String bilgisini `.env` / Environment Variable (`CAUSALCAMPAIGN_DB_CONNECTION`) üzerinden oku ve başlangıçta `ValidateOnStart()` ile doğrula.

4. **Test ve Dokümantasyon:**
   - Testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - Tüm API Controller uç noktalarına ve Global Exception Handler sınıfına **XML Documentation (`/// <summary>`)** ekle.

---

### BU AŞAMADA (`CausalCampaign.Web`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.Web` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve tüm alt projelerin (`Domain`, `Application`, `Infrastructure`, `PosEnrichment`, `CausalAi`, `Optimization`) Dependency Injection kayıtlarını `Program.cs` içinde birleştir.
   - Uygulama başlangıcında `SqlDatabaseBootstrapper` ile `CausalCampaignDb` veritabanı yoksa `CausalCampaignDb_Setup_And_Seed.sql` scriptini otomatik çalıştırarak hazırla.
2. **Merkezi Hata Yönetimi (`Middleware/GlobalExceptionHandler.cs`):**
   - `IExceptionHandler` uygulayıcısı: Doğrulama hatalarını `400 Bad Request`, kayıt bulunamama hatalarını `404 Not Found`, `RowVersion` eşzamanlılık çakışmalarını `409 Conflict`, Kill-Switch veya Anti-Gaming engellerini `422 Unprocessable Entity`, beklenmeyen hataları `500 Internal Server Error` (`ProblemDetails`) olarak döndür.
3. **REST API Controller (`Controllers/CampaignEngineController.cs`):**
   - `POST /api/v1/transactions/authorize`: Gerçek zamanlı ISO 8583 kart provizyonu alır, POS zenginleştirme + Anti-Gaming + Sayaç sonucunu ve `ProcessingLatencyMs` (`< 500 ms`) değerini döner.
   - `POST /api/v1/transactions/reversal`: İptal/iade mesajı alır, sayacı düşürür ve `Clawback` durumunu döner.
   - `POST /api/v1/campaigns/{campaignId}/optimize`: R-Learner Uplift + Google.OrTools MILP optimizasyonunu tetikler.
   - `POST /api/v1/offers/select-preference`: Müşterinin 3 eşdeğer tekliften seçimini kaydeder.
   - `POST /api/v1/campaigns/kill-switch`: Kampanya acil durdurma (`Kill-Switch`) tetikler.
   - `GET /api/v1/campaigns/{campaignId}/dashboard`: 8 KPI, CUPED, PSI Drift ve HDBSCAN özetini döner.
4. **3'ü 1 Arada İnteraktif Blazor Komuta Merkezi (`Components/Pages/Home.razor`):**
   Tek ekranda 3 sekmeden oluşan modern, Türkçe kurumsal arayüz:
   - **SEKME 1: 📱 Mobil Uygulama Müşteri Simülatörü**
     - Müşteri seçici (`Ahmet Yılmaz - Gold / Persuadable`, `Zeynep Kaya - Silver / Nakit İade Seven`, `Mehmet Demir - Private / Free-Rider`, `Elif Şahin - Gold / İnaktif Uyuyan Kart`).
     - Mobil telefon görünümü içinde:
       - **Kişiye Özel Teklif Kartı:** Harcama hedefi, ödül tutarı/tipi ve **XAI Türkçe Karar Gerekçesi**.
       - **Avantajını Kendin Seç (Active Preference Learning):** Banka açısından eşdeğer kârlılıktaki 3 teklif seçeneği (tıklandığında anında hedefi ve ödül tipini değiştiren interaktif seçim kartları).
       - **Canlı Hedef Sayacı (`Progress Bar`):** Harcanan tutar, kalan tutar, yüzde çubuğu ve işlem rozeti.
       - **Canlı POS İşlem & Anti-Gaming Simülasyon Butonları:**
         1. `"💳 Geçerli İşlem Geçir (IYZICO*TRENDYOL.COM - 4.500 TL)"`
         2. `"🚫 Anti-Gaming 1: 30 Dk İçinde Bölünmüş İşlem Geçir (PARAM*GETIR - 2.500 TL)"`
         3. `"🚫 Anti-Gaming 2: Nakit Benzeri Yasaklı MCC Geçir (ALTINBAS KUYUMCULUK MCC 6051 - 10.000 TL)"`
         4. `"↩️ İade / Refund Geçir (Ödül Geri Alma / Clawback Testi)"`
   - **SEKME 2: 🎛️ Back-Office Yönetim, 8 KPI, CUPED ve Kill-Switch Paneli**
     - 8 KPI Canlı Durum Kartları (`KPI-1` POS F1 `=> %92`, `KPI-2` Free-Rider `<= %25`, `KPI-3` CUPED ROI `=> 1.35x`, `KPI-4` Reactivation `=> %6.5`, `KPI-5` Latency `< 500 ms`, `KPI-6` MILP Süresi `< 15 dk`, `KPI-7` PSI `< 0.15`, `KPI-8` Kural İhlali `%0`).
     - **"🚀 R-Learner + MILP Portföy Optimizasyonunu Çalıştır"** butonu ve 20 Mikro-Kova dağılım tablosu.
     - **CUPED Net Ekonomik Katkı Raporu**, **Aylık PSI Drift İzleme**, **HDBSCAN Küme Özeti** ve **"🛑 ACİL DURDUR (KILL-SWITCH)"** butonu.
   - **SEKME 3: 🎧 Çağrı Merkezi Müşteri Gerekçe & Denetim Ekranı**
     - Müşteri temsilcisinin müşteriye neden bu hedefin atandığını (XAI öznitelik katkı çubuklarıyla), hangi işlemlerin sayaca dahil edildiğini ve hangi işlemlerin Anti-Gaming veya Clawback nedeniyle kapsam dışı kaldığını şeffafça gördüğü ekran.
5. **Birim Testler (`tests/CausalCampaign.UnitTests/WebTests/`):**
   - `MSTest` ve `Moq` kullanarak `CampaignEngineController` uç noktalarını, `GlobalExceptionHandler` RFC 7807 `ProblemDetails` dönüşlerini ve uçtan uca 8 KPI kriterini doğrulayan testleri yaz.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.Web` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `ASP.NET Core 10.0`, `Blazor Interactive Server`, `IExceptionHandler` (`ProblemDetails`), `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** Composition Root DI kurgusu, Global Error Handling yapısı ve 3'ü 1 arada Blazor ekranlarının Application katmanıyla etkileşimi.

Sonunda **"7. Aşama (`CausalCampaign.Web`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile tüm çözümün `%100` hatasız derlendiğini ve tüm testlerin geçtiğini doğrulayıp teslim et.
```
