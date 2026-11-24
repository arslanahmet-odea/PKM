# CausalCampaign — Dış Servisler ve 3. Parti Bağımlılık Raporu

Bu rapor, `CausalCampaign` çözümünün (`CausalCampaign.sln`) kaynak kodları, `.csproj` paket bağımlılıkları, konfigürasyon dosyaları (`appsettings.json`, `.env.example`) ve ön yüz (`App.razor`, `_Imports.razor`) katmanları statik olarak taranarak hazırlanmıştır.

> **Genel Değerlendirme:** `CausalCampaign`, **yalnızca tek bir harici ağ bağımlılığına (Microsoft SQL Server)** sahip, bunun dışındaki tüm yapay zeka, makine öğrenmesi, optimizasyon ve doğrulama işlemlerini **tamamen lokal (in-process)** olarak yürüten kapalı devre (air-gapped) bir mimariye sahiptir. Herhangi bir dış LLM API'si (OpenAI vb.), harici REST/gRPC servisi, mesaj kuyruğu, e-posta/SMS sağlayıcısı veya ön yüz CDN/tracking bağlantısı bulunmamaktadır.

---

## 1. Özet Tablo

| Servis / Sistem Adı | Kategori | İletişim Protokolü / SDK | Kullanılan Konumlar (Dosya & Sembol) | Durum |
| :--- | :--- | :--- | :--- | :--- |
| **Microsoft SQL Server** | Harici Veritabanı | TCP / TDS (`Microsoft.EntityFrameworkCore.SqlServer` 9.0.2, `Microsoft.Data.SqlClient`) | `CausalCampaign.Infrastructure/DependencyInjection.cs` (`UseSqlServer`), `Seeding/SqlDatabaseBootstrapper.cs` (`SqlConnection`) | **Aktif (Dış Ağ)** |
| **ML.NET & LightGBM & Recommender** | Makine Öğrenmesi (Lokal) | In-Process (`Microsoft.ML` 4.0.0, `Microsoft.ML.LightGbm` 4.0.0, `Microsoft.ML.Recommender` 0.22.0) | `CausalCampaign.CausalAi`, `CausalCampaign.PosEnrichment` | **Aktif (Lokal / Ağ Yok)** |
| **Google OR-Tools** | Optimizasyon / MILP (Lokal) | In-Process (`Google.OrTools` 9.11.4210) | `CausalCampaign.Optimization` | **Aktif (Lokal / Ağ Yok)** |
| **MathNet.Numerics** | İstatistik & Matematik (Lokal) | In-Process (`MathNet.Numerics` 5.0.0) | `CausalCampaign.CausalAi` | **Aktif (Lokal / Ağ Yok)** |
| **FluentValidation** | Veri Doğrulama (Lokal) | In-Process (`FluentValidation` 11.11.0) | `CausalCampaign.Application/Validators/*` | **Aktif (Lokal / Ağ Yok)** |
| **System.Net.Http (HttpClient)** | Harici HTTP İstemcisi | `.NET BCL` (`System.Net.Http`, `System.Net.Http.Json`) | `CausalCampaign.Web/Components/_Imports.razor` (sadece `@using` direktifi) | **Pasif (Kullanılmıyor)** |
| **EF Core InMemory / Moq / MSTest** | Test Altyapısı | In-Process (`Microsoft.EntityFrameworkCore.InMemory` 9.0.2, `Moq` 4.20.72) | `CausalCampaign.UnitTests` | **Aktif (Sadece Test)** |

---

## 2. Detaylı Bulgular

### 2.1. Aktif Dış Ağ Bağlantısı: Microsoft SQL Server
- **Protokol / Sürücü:** `Microsoft.EntityFrameworkCore.SqlServer` (`9.0.2`) ve `Microsoft.Data.SqlClient` (`SqlConnection` / `SqlCommand`).
- **Konfigürasyon Anahtarları (Secret Değerleri Hariç):**
  - Ortam değişkeni: `CAUSALCAMPAIGN_DB_CONNECTION` (`.env` üzerinden `DotEnvLoader.LoadFromHierarchy()` ile okunur; şablon `.env.example` içinde tanımlı).
  - Ortam değişkeni: `CAUSALCAMPAIGN_TABLE_PREFIX`, `CAUSALCAMPAIGN_COMMAND_TIMEOUT_SECONDS`.
  - JSON Konfigürasyon (`src/CausalCampaign.Web/appsettings.json`): `Database:TablePrefix`, `Database:CommandTimeoutSeconds`, `Database:EnableSensitiveDataLogging`.
- **Kod Konumları:**
  - `src/CausalCampaign.Infrastructure/DependencyInjection.cs` — `AddCausalCampaignInfrastructure` içinde `dbOptions.UseSqlServer(options.ConnectionString, ...)` (retry ve timeout politikalarıyla).
  - `src/CausalCampaign.Infrastructure/Configuration/DatabaseOptions.cs` — `[Required]` ve `[MinLength(15)]` veri doğrulama kuralları.
  - `src/CausalCampaign.Infrastructure/Seeding/SqlDatabaseBootstrapper.cs` — `ExecuteSetupScriptAsync` içinde `new SqlConnection(_options.ConnectionString)` ile T-SQL kurulum ve seed betiğini (`GO` bloklarına bölerek) çalıştırır.
  - `tests/CausalCampaign.UnitTests/InfrastructureTests/SqlServerIntegrationAndSeederTests.cs` — Yapılandırılmışsa gerçek SQL Server'a bağlanan entegrasyon testi.

### 2.2. Lokal (In-Process) 3. Parti Kütüphaneler (Dış Ağ Çağrısı Yok)
- **ML.NET (`4.0.0`), `Microsoft.ML.LightGbm` (`4.0.0`), `Microsoft.ML.Recommender` (`0.22.0`):**
  - `CausalCampaign.CausalAi` ve `CausalCampaign.PosEnrichment` projelerinde kullanılır. Tüm eğitim ve tahmin (inference) süreçleri uygulama belleğinde çalışır; harici bir AI/ML API'sine istek göndermez.
- **Google OR-Tools (`9.11.4210`):**
  - `CausalCampaign.Optimization` projesinde MILP portföy optimizasyonu için kullanılır. İsminde "Google" geçmesine rağmen tamamen lokal C++/C# kütüphanesidir; Google Cloud veya herhangi bir dış API ile haberleşmez.
- **MathNet.Numerics (`5.0.0`) & FluentValidation (`11.11.0`):**
  - Sırasıyla `CausalCampaign.CausalAi` ve `CausalCampaign.Application` katmanlarında lokal hesaplama ve doğrulama için kullanılır.

### 2.3. Web Katmanı ve Gelen/Giden Bağlantılar (`CausalCampaign.Web`)
- **Gelen Bağlantılar (Inbound):** `Program.cs` üzerinde `app.MapControllers()` (REST API) ve `app.MapRazorComponents<App>().AddInteractiveServerRenderMode()` (Blazor Interactive Server / dahili SignalR devresi) sunulur.
- **Ön Yüz (UI):** `src/CausalCampaign.Web/Components/App.razor` dosyasında harici CDN, Google Fonts, dış JS kütüphanesi veya analytics/tracking script'i **yoktur**. Yalnızca dahili `_framework/blazor.web.js` ve inline CSS kullanılır.
- **Pasif HTTP Referansı:** `src/CausalCampaign.Web/Components/_Imports.razor` dosyasında `@using System.Net.Http` ve `@using System.Net.Http.Json` satırları Blazor şablonundan kalma olarak yer alır; ancak `src/` altında aktif bir `HttpClient`, `IHttpClientFactory`, `RestSharp`, `Refit` veya `gRPC` çağrısı yoktur.

---

## 3. Kör Noktalar (Statik Analiz Sınırları)
1. **Çalışma Zamanı Bağlantı Hedefi (`.env`):** Güvenlik gereği `.env` dosyasının gerçek içeriği rapora dahil edilmemiştir. `CAUSALCAMPAIGN_DB_CONNECTION` değeri çalışma zamanında yerel `localhost` yerine uzak bir kurumsal SQL Server sunucusunu (ör. `OMV3DVPDBLS,1469`) işaret edebilir.
2. **Altyapı / Ters Proxy Katmanı:** `appsettings.json` içinde `"AllowedHosts": "*"` tanımlıdır. Uygulamanın önüne konabilecek IIS, Nginx, API Gateway veya kimlik doğrulama katmanları kod tabanının dışında (deployment ortamında) yönetilmektedir.
