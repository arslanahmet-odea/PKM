# PROJE 2 PROMPTU: `CausalCampaign.Infrastructure` (EF Core 10.0 & SQL Server Veri Katmanı)

*(1. Projeyi inceleyip onayladıktan sonra, 2. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0 ve Microsoft SQL Server konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **2. Aşamasına (`src/CausalCampaign.Infrastructure` — Entity Framework Core 10.0 Veri Erişim Katmanı, Derlenmiş Sorgular ve Sentetik Portföy Üretici)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`
Veritabanı Kurulum Scripti: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka\CausalCampaign_Prompts\00_Database\CausalCampaignDb_Setup_And_Seed.sql`
SQL Server Connection String (.env / Environment Variable üzerinden okunacak):
`CAUSALCAMPAIGN_DB_CONNECTION="CONNECTION_STRING_BURAYA"`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını koru (`CausalCampaign.Infrastructure` yalnızca `CausalCampaign.Domain` katmanına referans verir).
   - Veri erişiminde **Dapper KESİNLİKLE KULLANILMAYACAK**, tüm işlemler **`Entity Framework Core 10.0` (`Microsoft.EntityFrameworkCore.SqlServer`)** üzerinden yapılacaktır.
   - Veritabanı adı `CausalCampaignDb`'dir. Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
   - `< 500 ms` gerçek zamanlı sayaç performansı (`KPI-5`) için sıcak yollarda (hot paths) **`EF.CompileAsyncQuery`** ile önceden derlenmiş sorgular ve salt-okunur analizlerde `AsNoTracking()` kullan.

3. **Hata Yönetimi ve Güvenlik:**
   - **Sıfır Hardcoded Secret:** Connection String asla C# koduna gömülmemelidir. `.env` / Environment Variable + `DatabaseOptions` sınıfı ve `IOptions<DatabaseOptions>` üzerinde **`ValidateDataAnnotations()` + `ValidateOnStart()`** kullan.
   - EF Core `DbUpdateConcurrencyException` (`RowVersion` çakışması) ve `SqlException` hatalarını yakalayıp `Result<T>` desenine dönüştüren güvenli Persistence sarmalayıcıları yaz.

4. **Test ve Dokümantasyon:**
   - Birim ve entegrasyon testleri için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - Karmaşık EF Core derlenmiş sorguları ve veri üretici metotlar için **XML Documentation (`/// <summary>`)** açıklamaları yaz.

---

### BU AŞAMADA (`CausalCampaign.Infrastructure`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.Infrastructure` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve `CausalCampaign.Domain` referansını bağla.
   - Paketler: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Extensions.Options.ConfigurationExtensions`, `Microsoft.Extensions.Options.DataAnnotations`.
2. **Yapılandırma ve Güvenlik (`Configuration/`):**
   - `DatabaseOptions.cs`: `[Required]` doğrulamalı `ConnectionString`, `CommandTimeoutSeconds`, `EnableSensitiveDataLogging` ayarları ve `.env` okuyucu (`DotEnvLoader.cs`).
3. **EF Core 10.0 DbContext ve Fluent API Eşlemeleri (`Persistence/` & `Persistence/Configurations/`):**
   - `CausalCampaignDbContext.cs` (`ICausalCampaignDbContext` uygulayıcısı).
   - 8 tablo (`CasualCampaign_CAMPAIGN_TEMPLATE`, `CasualCampaign_CUSTOMER_PROFILE_MIS`, `CasualCampaign_INFLATION_INDEX_MONTHLY`, `CasualCampaign_MERCHANT_MASTER_CACHE`, `CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT`, `CasualCampaign_TRANSACTION_LEDGER_FEED`, `CasualCampaign_OFFER_PROGRESS_TRACKER`, `CasualCampaign_REWARD_FULFILLMENT_QUEUE`) ve `vw_CasualCampaign_CustomerCausalFeatures` görünümü (`ToView("vw_CasualCampaign_CustomerCausalFeatures")`) için ayrı ayrı `IEntityTypeConfiguration<T>` sınıfları (Yeni DB açılmayacak, mevcut `DB_Odeabank_DVP_Cards` veritabanında `CasualCampaign_` ön ekli tablolar kullanılacaktır).
   - `OfferProgressTracker.RowVersion` üzerinde `.IsRowVersion()` iyimser eşzamanlılık koruması.
4. **Yüksek Performanslı Derlenmiş Sorgular (`Persistence/CompiledQueries/CausalCampaignCompiledQueries.cs`):**
   - `EF.CompileAsyncQuery` kullanarak `< 500 ms` gerçek zamanlı provizyon akışı için:
     - `GetMerchantCacheByKeyAsync`
     - `GetActiveTrackerByCustomerAndCampaignAsync`
     - `HasRecentMerchantTransactionWithinWindowAsync` (30 dk bölünmüş işlem Anti-Gaming kontrolü)
     - `GetPendingRewardByTrackerIdAsync` (`Clawback` kontrolü)
5. **Veritabanı Kurucu ve Sentetik Portföy Üretici (`Seeding/`):**
   - `SqlDatabaseBootstrapper.cs`: `CausalCampaignDb_Setup_And_Seed.sql` dosyasını (`GO` bloklarını ayrıştırarak) SQL Server üzerinde çalıştırıp veritabanını ve tabloları sıfırdan kuran servis.
   - `EfCoreSyntheticPortfolioGenerator.cs`: Test ve model eğitimi için portföyü 1.000+ müşteriye (Persuadable, FreeRider_SureThing, Dormant_Sleeper, LostCause personaları, 12 aylık enflasyonlu işlem geçmişi ve `IYZICO*`/`PARAM*`/`PAYTR*` işlemleri) genişleten deterministik tohumlu (`Random(42)`) EF Core veri üreticisi.
6. **Dependency Injection Kaydı (`DependencyInjection.cs`):**
   - `AddCausalCampaignInfrastructure(IConfiguration)` uzantı metodu.
7. **Birim Testler (`tests/CausalCampaign.UnitTests/InfrastructureTests/`):**
   - `MSTest` ve `Moq` kullanarak `DatabaseOptions` doğrulama testleri, `CausalCampaignDbContext` Fluent API model doğrulama testleri, derlenmiş sorgu testleri ve `EfCoreSyntheticPortfolioGenerator` veri dağılım testleri.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.Infrastructure` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `EF Core 10.0`, `EF.CompileAsyncQuery`, `IOptions` doğrulaması, `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** Dapper yerine EF Core Compiled Queries ile `< 500 ms` gecikmenin nasıl sağlanacağı, `.env` güvenlik yapısı ve `RowVersion` concurrency yönetimi.

Sonunda **"2. Aşama (`CausalCampaign.Infrastructure`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile doğrulayıp incelemem için dur.
```
