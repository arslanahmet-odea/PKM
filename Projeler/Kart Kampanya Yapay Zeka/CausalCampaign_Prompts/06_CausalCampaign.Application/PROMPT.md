# PROJE 6 PROMPTU: `CausalCampaign.Application` (Gerçek Zamanlı Sayaç `< 500 ms`, Anti-Gaming, Clawback & FluentValidation Orkestrasyon Katmanı)

*(5. Projeyi inceleyip onayladıktan sonra, 6. Proje için bu dosyayı kopyalayıp doğrudan Antigravity'ye yapıştırın.)*

```text
Sen .NET 10.0, Clean Architecture Uygulama Katmanı (`Application Layer`), `FluentValidation` ve `Entity Framework Core 10.0` konusunda uzman, kıdemli bir yazılım mimarısın (`Senior .NET Solutions Architect`).
`CausalCampaign` ("Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme Destekli Dinamik Kampanya Karar Motoru") projesinin **6. Aşamasına (`src/CausalCampaign.Application` — Gerçek Zamanlı ISO 8583 Provizyon Sayaç Motoru, Anti-Gaming Filtreleri, İade Clawback, FluentValidation Girdi Doğrulaması ve Kampanya Orkestrasyon Katmanı)** geçiyoruz.

Çalışma Dizinimiz: `c:\Users\obu50550\Documents\PKM\Projeler\Kart Kampanya Yapay Zeka`

---

### KESİN MÜHENDİSLİK STANDARTLARI VE "BEST PRACTICES" KURALLARI

1. **Mimari ve Klasör Yapısı:**
   - **Clean Architecture** yapısını koru (`CausalCampaign.Application`, kullanım senaryolarını / Use Case'leri orkestre eder; yalnızca `CausalCampaign.Domain` arayüzlerine bağımlıdır).
   - Veritabanı işlemlerinde Dapper kullanılmaz; yalnızca `ICausalCampaignDbContext` (EF Core 10.0) kullanılır.
   - Hiçbir yerde kurum veya eski proje kodu geçmemelidir.

2. **Kod Kalitesi ve Prensipler:**
   - **SOLID, DRY ve KISS** prensiplerine sadık kal.
   - **Katı Tip Güvenliği (Strict Typing):** `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

3. **Hata Yönetimi, Girdi Doğrulama ve Güvenlik:**
   - **Tüm Dış Girdilerin Doğrulanması (`FluentValidation`):** Sisteme giren tüm provizyon, iade, kampanya optimizasyonu, tercih seçimi ve Kill-Switch isteklerini **`FluentValidation` (`AbstractValidator<T>`)** ile doğrula (Zod/Pydantic'in .NET karşılığı). Geçersiz girdileri veritabanına gitmeden `Result<T>.Failure(DomainError.Validation(...))` olarak reddet.
   - İyimser eşzamanlılık (`DbUpdateConcurrencyException` / `RowVersion`) çakışmalarını güvenli yeniden deneme (retry) veya temiz hata dönüşüyle yönet.

4. **Test ve Dokümantasyon:**
   - Testler için yalnızca **`Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest)** ve **`Moq`** kullan.
   - Tüm Use Case servislerine ve Validator sınıflarına **XML Documentation (`/// <summary>`)** açıklamaları yaz.

---

### BU AŞAMADA (`CausalCampaign.Application`) İNŞA EDİLECEKLER

1. **Proje Kurulumu (`src/CausalCampaign.Application` - `net10.0`):**
   - `CausalCampaign.sln` çözümüne ekle ve `CausalCampaign.Domain` referansını bağla.
   - Paket: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.
2. **Girdi Doğrulayıcılar (`Validators/` — `FluentValidation`):**
   - `AuthorizeTransactionRequestValidator`: `CustomerId > 0`, `Amount > 0`, `AuthorizationCode` boş olamaz, `RawMcc_DE18` 4 haneli sayısal kod olmalı, `RawMerchantName_DE43` en az 3 karakter olmalı.
   - `ReversalTransactionRequestValidator`: `CustomerId > 0`, `RefundAmount > 0`, `OriginalTransactionId` doğrulaması.
   - `SelectAlternativeOfferRequestValidator`: `AssignmentId > 0`, `SelectedOfferIndex` yalnızca `1`, `2` veya `3` olabilir.
   - `TriggerKillSwitchRequestValidator`: `CampaignId > 0`, `Reason` en az 10 karakter olmalı.
3. **Gerçek Zamanlı Provizyon Sayaç ve Anti-Gaming Servisi (`Services/RealtimeOfferTrackerService.cs`):**
   - ISO 8583 `0100/0200` kart provizyon mesajı geldiğinde `Stopwatch` ile gecikmeyi ölçerek **`< 500 ms` (`KPI-5`)** altında çalışan akış:
     - **Adım 1 (Girdi Doğrulama):** `AuthorizeTransactionRequestValidator` ile doğrula.
     - **Adım 2 (POS Zenginleştirme):** `IPosEnrichmentService` ile işlemin gerçek marka (`ResolvedBrand`) ve sektörünü (`ResolvedSector`) çözümle.
     - **Adım 3 (Kill-Switch Kontrolü):** Müşterinin aktif kampanyasında `KillSwitchTriggered == true` ise sayacı ilerletme.
     - **Adım 4 (Anti-Gaming Kural 1 — Nakit Benzeri / Yasaklı MCC):** İşlem MCC kodu nakit çekim, altın/kuyum, döviz, bahis veya para transferi (`6011`, `6012`, `6051`, `7995`, `4829`) ise `IsExcludedByAntiGaming = true`, `ExclusionReason = "Anti-Gaming: Nakit benzeri / kampanya dışı MCC kodu"` olarak kaydet ve sayaca dahil etme.
     - **Adım 5 (Anti-Gaming Kural 2 — 30 Dk Bölünmüş İşlem / Split-Transaction Engeli):** Aynı müşteri aynı üye işyerinde (veya aynı `ResolvedBrand` markasında) son 30 dakika içinde geçerli bir işlem yapmışsa `IsExcludedByAntiGaming = true`, `ExclusionReason = "Anti-Gaming: Aynı üye işyerinde 30 dakika içinde tespit edilen bölünmüş işlem (Split-Transaction)"` olarak kaydet, `ExcludedTxCount` değerini artır ve sayacı artırma.
     - **Adım 6 (Atomik Sayaç Güncelleme & Ödül Hak Edişi):** İşlem geçerliyse `OFFER_PROGRESS_TRACKER` üzerinde `CurrentQualifiedSpend`, `RemainingAmount`, `ProgressPercentage` ve `QualifiedTxCount` alanlarını güncelle. `CurrentQualifiedSpend => TargetAmount` olduğu anda `IsCompleted = true` yap ve `REWARD_FULFILLMENT_QUEUE` tablosuna `PendingSettlement` statüsünde ödül kaydı aç.
4. **İptal / İade ve Ödül Geri Alma (`Clawback`) Akışı (`ProcessReversalOrRefundAsync`):**
   - İade işlemi geldiğinde `OFFER_PROGRESS_TRACKER` sayacından iade tutarını düş.
   - Eğer güncel geçerli harcama `TargetAmount` altına inerse `IsCompleted = false` yap ve `REWARD_FULFILLMENT_QUEUE` kaydını `Status = "ClawedBack"`, `ClawbackReason = "İade kaynaklı harcama hedefi altına düşüş"` olarak güncelle.
5. **Kampanya Orkestrasyon ve Yönetim Servisi (`Services/CampaignManagementAppService.cs`):**
   - `RunEndToEndCampaignOptimizationAsync(int campaignId)`: Feature Store -> R-Learner Uplift -> XAI Gerekçe -> MILP Optimizasyon -> Stratified Hold-out atamasını uçtan uca çalıştırıp EF Core ile kaydeden orkestratör metot.
   - `SelectCustomerOfferPreferenceAsync(SelectAlternativeOfferRequestDto request)`: Müşterinin 3 eşdeğer tekliften seçimini kaydeden ve sayacın hedefini seçilen pakete göre güncelleyen metot.
   - `TriggerCampaignKillSwitchAsync(TriggerKillSwitchRequestDto request)`: Acil durdurma (`Kill-Switch`) tetikleyen metot.
   - `GetExecutiveDashboardSummaryAsync(int campaignId)`: 8 KPI'ın tamamını, CUPED raporunu, PSI Drift durumunu ve HDBSCAN küme özetini tek DTO'da toplayan metot.
6. **Birim Testler (`tests/CausalCampaign.UnitTests/ApplicationTests/`):**
   - `MSTest` ve `Moq` kullanarak:
     - `FluentValidation` doğrulama testleri,
     - Geçerli işlemde sayacın doğru arttığı ve hedef tamamlanınca `REWARD_FULFILLMENT_QUEUE` kaydı açıldığı,
     - 30 dk içindeki 2. bölünmüş işlemin ve `6051` kuyum/nakit benzeri MCC işleminin `%100` engellendiği (`KPI-8`),
     - İade geldiğinde ödülün `ClawedBack` statüsüne çekildiği,
     - İşlem gecikmesinin `< 500 ms` (`KPI-5`) olduğu ve `Kill-Switch` aktifken sayacın durduğu senaryoların doğrulanması.

---

### ÇALIŞMA ŞEKLİMİZ (DİKKAT: ŞİMDİ HEMEN KOD YAZMA!)

İlk olarak hiçbir C# kodu yazmadan bana şu 3 başlığı sun ve onayımı bekle:
1. **Önerdiğin Klasör Yapısı (Directory Tree):** `CausalCampaign.Application` içindeki tüm dosya ağacı.
2. **Kullanacağın Temel Kütüphaneler ve Seçilme Nedenleri:** `FluentValidation`, EF Core 10.0 `ICausalCampaignDbContext`, `MSTest`, `Moq`.
3. **Mimari Kararların Özeti:** `< 500 ms` gerçek zamanlı provizyon boru hattı, Anti-Gaming & Clawback state machine tasarımı ve `FluentValidation` + `Result<T>` hata yönetimi.

Sonunda **"6. Aşama (`CausalCampaign.Application`) planını onaylıyor musunuz?"** diye sor. Ben onay verdikten sonra tüm kodları eksiksiz yaz, `dotnet build` ve `dotnet test` ile doğrulayıp incelemem için dur.
```
