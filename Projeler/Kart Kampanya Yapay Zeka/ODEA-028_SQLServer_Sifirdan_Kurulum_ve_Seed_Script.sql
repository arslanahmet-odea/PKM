/* =========================================================================================
   PROJE KODU : ODEA-028
   PROJE ADI  : Kartlı Ödeme Süreçlerinde Nedensel Makine Öğrenmesi ve Pekiştirmeli Öğrenme
                Destekli Dinamik Kampanya Karar Motoru Geliştirilmesi
   DOSYA      : ODEA-028_SQLServer_Sifirdan_Kurulum_ve_Seed_Script.sql
   AÇIKLAMA   : Microsoft SQL Server üzerinde veritabanını (Odea028_CausalCampaignDb),
                8 çekirdek tabloyu, performans indekslerini, enflasyon-düzeltmeli Feature Store
                görünümünü (vw_CustomerCausalFeatures) ve gerçekçi başlangıç (seed) verilerini
                sıfırdan oluşturur.
   ========================================================================================= */

SET NOCOUNT ON;
GO

-- 1. VERİTABANI OLUŞTURMA
IF DB_ID(N'Odea028_CausalCampaignDb') IS NULL
BEGIN
    CREATE DATABASE [Odea028_CausalCampaignDb];
    PRINT N'Odea028_CausalCampaignDb veritabanı oluşturuldu.';
END
GO

USE [Odea028_CausalCampaignDb];
GO

-- Mevcut Görünüm ve Tabloları Temizle (Yeniden Çalıştırılabilirlik / Idempotency İçin)
IF OBJECT_ID(N'dbo.vw_CustomerCausalFeatures', N'V') IS NOT NULL DROP VIEW dbo.vw_CustomerCausalFeatures;
IF OBJECT_ID(N'dbo.REWARD_FULFILLMENT_QUEUE', N'U') IS NOT NULL DROP TABLE dbo.REWARD_FULFILLMENT_QUEUE;
IF OBJECT_ID(N'dbo.OFFER_PROGRESS_TRACKER', N'U') IS NOT NULL DROP TABLE dbo.OFFER_PROGRESS_TRACKER;
IF OBJECT_ID(N'dbo.TRANSACTION_LEDGER_FEED', N'U') IS NOT NULL DROP TABLE dbo.TRANSACTION_LEDGER_FEED;
IF OBJECT_ID(N'dbo.CUSTOMER_OFFER_ASSIGNMENT', N'U') IS NOT NULL DROP TABLE dbo.CUSTOMER_OFFER_ASSIGNMENT;
IF OBJECT_ID(N'dbo.MERCHANT_MASTER_CACHE', N'U') IS NOT NULL DROP TABLE dbo.MERCHANT_MASTER_CACHE;
IF OBJECT_ID(N'dbo.INFLATION_INDEX_MONTHLY', N'U') IS NOT NULL DROP TABLE dbo.INFLATION_INDEX_MONTHLY;
IF OBJECT_ID(N'dbo.CUSTOMER_PROFILE_MIS', N'U') IS NOT NULL DROP TABLE dbo.CUSTOMER_PROFILE_MIS;
IF OBJECT_ID(N'dbo.CAMPAIGN_TEMPLATE', N'U') IS NOT NULL DROP TABLE dbo.CAMPAIGN_TEMPLATE;
GO

/* =========================================================================================
   2. TABLO ŞEMALARI (EF CORE 10.0 UYUMLU 8 ÇEKİRDEK TABLO)
   ========================================================================================= */

-- TABLO 1: CAMPAIGN_TEMPLATE (Kampanya Şablonları, Bütçe, Kısıtlar ve Kill-Switch)
CREATE TABLE dbo.CAMPAIGN_TEMPLATE (
    CampaignId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CAMPAIGN_TEMPLATE PRIMARY KEY,
    CampaignCode            NVARCHAR(30)      NOT NULL CONSTRAINT UQ_CAMPAIGN_TEMPLATE_Code UNIQUE,
    CampaignName            NVARCHAR(200)     NOT NULL,
    CampaignType            NVARCHAR(40)      NOT NULL, -- 'TotalSpendUpsell', 'SectoralCrossSell', 'DormantReactivation'
    TargetSector            NVARCHAR(80)      NOT NULL, -- 'Tümü', 'Market & Gıda', 'Akaryakıt', 'E-Ticaret & Pazaryeri', 'Giyim & Tekstil' vb.
    DefaultRewardType       NVARCHAR(30)      NOT NULL, -- 'OdeaPuan', 'Cashback'
    TotalBudget             DECIMAL(18,2)     NOT NULL,
    ConsumedBudget          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CAMPAIGN_ConsumedBudget DEFAULT (0.00),
    MaxRewardPerCustomer    DECIMAL(18,2)     NOT NULL,
    MinMisProfitThreshold   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CAMPAIGN_MinMis DEFAULT (-500.00),
    InterchangeMarginRate   DECIMAL(9,4)      NOT NULL CONSTRAINT DF_CAMPAIGN_Margin DEFAULT (0.0250), -- %2.50 marj
    StartDate               DATETIME2(3)      NOT NULL,
    EndDate                 DATETIME2(3)      NOT NULL,
    IsActive                BIT               NOT NULL CONSTRAINT DF_CAMPAIGN_IsActive DEFAULT (1),
    KillSwitchTriggered     BIT               NOT NULL CONSTRAINT DF_CAMPAIGN_KillSwitch DEFAULT (0),
    KillSwitchReason        NVARCHAR(300)     NULL,
    CreatedBy               NVARCHAR(100)     NOT NULL CONSTRAINT DF_CAMPAIGN_CreatedBy DEFAULT (N'KORAY AK'),
    CreatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_CAMPAIGN_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- TABLO 2: CUSTOMER_PROFILE_MIS (Müşteri Kart Profili, Boş Limit, MIS Kârlılık ve Segment)
CREATE TABLE dbo.CUSTOMER_PROFILE_MIS (
    CustomerId              INT IDENTITY(1001,1) NOT NULL CONSTRAINT PK_CUSTOMER_PROFILE_MIS PRIMARY KEY,
    CustomerExternalNo      NVARCHAR(30)         NOT NULL CONSTRAINT UQ_CUSTOMER_ExternalNo UNIQUE,
    FullName                NVARCHAR(120)        NOT NULL,
    CardNumberMasked        NVARCHAR(25)         NOT NULL,
    PrivilegesSegment       NVARCHAR(50)         NOT NULL, -- 'Standart', 'Ayrıcalıklar Dünyası Gümüş', 'Ayrıcalıklar Dünyası Altın', 'Ayrıcalıklar Dünyası Özel'
    TotalCreditLimit        DECIMAL(18,2)        NOT NULL,
    AvailableCreditLimit    DECIMAL(18,2)        NOT NULL,
    MisMonthlyNetProfit     DECIMAL(18,2)        NOT NULL, -- Banka geneli mevduat+kredi+kart MIS net katkısı (TL)
    HistoricalBaselineSpend DECIMAL(18,2)        NOT NULL, -- Son 3 ay ortalama nominal harcama (TL)
    RealBaselineSpendIndex  DECIMAL(18,2)        NOT NULL, -- Enflasyondan arındırılmış reel harcama (TL)
    SpendElasticityPersona  NVARCHAR(40)         NOT NULL, -- 'Persuadable', 'FreeRider_SureThing', 'Dormant_Sleeper', 'LostCause'
    IsDormant               BIT                  NOT NULL CONSTRAINT DF_CUSTOMER_IsDormant DEFAULT (0),
    DaysSinceLastTx         INT                  NOT NULL CONSTRAINT DF_CUSTOMER_DaysSinceLastTx DEFAULT (0),
    HasKvkkIysConsent       BIT                  NOT NULL CONSTRAINT DF_CUSTOMER_Consent DEFAULT (1),
    PreferredRewardType     NVARCHAR(30)         NOT NULL CONSTRAINT DF_CUSTOMER_PrefReward DEFAULT (N'OdeaPuan'),
    BehavioralClusterId     INT                  NOT NULL CONSTRAINT DF_CUSTOMER_Cluster DEFAULT (1), -- HDBSCAN Küme ID
    CreatedAt               DATETIME2(3)         NOT NULL CONSTRAINT DF_CUSTOMER_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- TABLO 3: INFLATION_INDEX_MONTHLY (Sektörel Aylık Enflasyon ve Reel Deflatör Tablosu)
CREATE TABLE dbo.INFLATION_INDEX_MONTHLY (
    InflationIndexId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_INFLATION_INDEX_MONTHLY PRIMARY KEY,
    YearMonth               INT               NOT NULL, -- Örn: 202606..202611
    SectorName              NVARCHAR(80)      NOT NULL,
    CpiIndexValue           DECIMAL(12,4)     NOT NULL, -- Baz ay = 100.0000
    DeflatorMultiplier      DECIMAL(12,6)     NOT NULL, -- Güncel aya getirme çarpanı (CurrentIndex / MonthIndex)
    CONSTRAINT UQ_INFLATION_YearMonth_Sector UNIQUE (YearMonth, SectorName)
);
GO

-- TABLO 4: MERCHANT_MASTER_CACHE (3 Katmanlı POS NLP/SLM Semantik Sınıflandırma Önbelleği)
CREATE TABLE dbo.MERCHANT_MASTER_CACHE (
    RawMerchantKey          NVARCHAR(250)     NOT NULL CONSTRAINT PK_MERCHANT_MASTER_CACHE PRIMARY KEY,
    RawMcc_DE18             NVARCHAR(10)      NOT NULL,
    CleanedBrandName        NVARCHAR(120)     NOT NULL,
    ResolvedSector          NVARCHAR(80)      NOT NULL,
    SubMerchantProvider     NVARCHAR(60)      NULL,     -- 'IYZICO', 'PARAM', 'PAYTR', 'SIPAY', 'MOKA', NULL
    ClassificationLayer     TINYINT           NOT NULL, -- 1=Deterministik Sözlük, 2=Ödeme Kuruluşu Regex, 3=ML.NET/SLM
    ConfidenceScore         DECIMAL(6,4)      NOT NULL,
    IsCashLikeOrExcludedMcc BIT               NOT NULL CONSTRAINT DF_MERCHANT_IsExcluded DEFAULT (0),
    UpdatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_MERCHANT_UpdatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- TABLO 5: CUSTOMER_OFFER_ASSIGNMENT (R-Learner + MILP Ataması, 3 Eşdeğer Teklif ve SHAP/XAI Gerekçesi)
CREATE TABLE dbo.CUSTOMER_OFFER_ASSIGNMENT (
    AssignmentId                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CUSTOMER_OFFER_ASSIGNMENT PRIMARY KEY,
    CampaignId                    INT               NOT NULL CONSTRAINT FK_ASSIGNMENT_Campaign FOREIGN KEY REFERENCES dbo.CAMPAIGN_TEMPLATE(CampaignId),
    CustomerId                    INT               NOT NULL CONSTRAINT FK_ASSIGNMENT_Customer FOREIGN KEY REFERENCES dbo.CUSTOMER_PROFILE_MIS(CustomerId),
    MicroBucketId                 INT               NOT NULL, -- 1..20 arası standart mikro-kova
    MicroBucketName               NVARCHAR(100)     NOT NULL,
    ExperimentGroup               NVARCHAR(30)      NOT NULL, -- 'Treatment', 'Control_Holdout'
    ExpectedOrganicSpend          DECIMAL(18,2)     NOT NULL, -- R-Learner 1. Aşama: Kampanyasız beklenen harcama
    PredictedUpliftSpend          DECIMAL(18,2)     NOT NULL, -- R-Learner 2. Aşama: Kampanya kaynaklı net ilave harcama
    PropensityScore               DECIMAL(6,4)      NOT NULL,
    ReactivationProbability       DECIMAL(6,4)      NOT NULL CONSTRAINT DF_ASSIGNMENT_ReactProb DEFAULT (0.0000),
    AssignedTargetAmount          DECIMAL(18,2)     NOT NULL, -- MILP tarafından atanan harcama hedefi (TL)
    AssignedRewardAmount          DECIMAL(18,2)     NOT NULL, -- MILP tarafından atanan ödül (TL)
    RewardType                    NVARCHAR(30)      NOT NULL, -- 'OdeaPuan', 'Cashback'
    ExpectedNetIncrementalProfit  DECIMAL(18,2)     NOT NULL, -- (PredictedUpliftSpend * Marj) - AssignedRewardAmount
    XaiReasonText                 NVARCHAR(500)     NOT NULL, -- Müşteri ve Çağrı Merkezi için Türkçe karar gerekçesi
    XaiTopFeaturesJson            NVARCHAR(1000)    NOT NULL, -- Öznitelik katkı ağırlıkları JSON
    AlternativeOffersJson         NVARCHAR(2000)    NOT NULL, -- Eşdeğer kârlılıklı 3 teklif seçeneği (Tercih Öğrenme)
    CustomerSelectedOfferIndex    TINYINT           NOT NULL CONSTRAINT DF_ASSIGNMENT_SelectedIdx DEFAULT (1), -- 1, 2 veya 3
    IsApprovedByHuman             BIT               NOT NULL CONSTRAINT DF_ASSIGNMENT_Approved DEFAULT (1),
    AssignedAt                    DATETIME2(3)      NOT NULL CONSTRAINT DF_ASSIGNMENT_AssignedAt DEFAULT (SYSUTCDATETIME())
);
CREATE INDEX IX_ASSIGNMENT_Customer_Campaign ON dbo.CUSTOMER_OFFER_ASSIGNMENT(CustomerId, CampaignId);
GO

-- TABLO 6: TRANSACTION_LEDGER_FEED (ISO 8583 Provizyon/Takas Akışı ve Anti-Gaming Denetim İzi)
CREATE TABLE dbo.TRANSACTION_LEDGER_FEED (
    TransactionId           BIGINT IDENTITY(100001,1) NOT NULL CONSTRAINT PK_TRANSACTION_LEDGER_FEED PRIMARY KEY,
    AuthorizationCode       NVARCHAR(20)              NOT NULL,
    CustomerId              INT                       NOT NULL CONSTRAINT FK_TX_Customer FOREIGN KEY REFERENCES dbo.CUSTOMER_PROFILE_MIS(CustomerId),
    TransactionTimestamp    DATETIME2(3)              NOT NULL,
    Amount                  DECIMAL(18,2)             NOT NULL,
    Currency                NVARCHAR(10)              NOT NULL CONSTRAINT DF_TX_Currency DEFAULT (N'TRY'),
    RawMcc_DE18             NVARCHAR(10)              NOT NULL,
    RawMerchantName_DE43    NVARCHAR(250)             NOT NULL,
    ResolvedBrand           NVARCHAR(120)             NOT NULL,
    ResolvedSector          NVARCHAR(80)              NOT NULL,
    ClassificationLayer     TINYINT                   NOT NULL CONSTRAINT DF_TX_Layer DEFAULT (1),
    IsReversalOrRefund      BIT                       NOT NULL CONSTRAINT DF_TX_IsRefund DEFAULT (0),
    OriginalTransactionId   BIGINT                    NULL,
    IsExcludedByAntiGaming  BIT                       NOT NULL CONSTRAINT DF_TX_IsExcluded DEFAULT (0),
    ExclusionReason         NVARCHAR(250)             NULL,
    ProcessingLatencyMs     DECIMAL(10,2)             NOT NULL CONSTRAINT DF_TX_Latency DEFAULT (12.50)
);
CREATE INDEX IX_TX_Customer_Timestamp ON dbo.TRANSACTION_LEDGER_FEED(CustomerId, TransactionTimestamp DESC);
GO

-- TABLO 7: OFFER_PROGRESS_TRACKER (< 500 ms Gerçek Zamanlı Hedef Sayacı ve İyimser Eşzamanlılık)
CREATE TABLE dbo.OFFER_PROGRESS_TRACKER (
    TrackerId               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OFFER_PROGRESS_TRACKER PRIMARY KEY,
    AssignmentId            INT               NOT NULL CONSTRAINT UQ_TRACKER_Assignment UNIQUE CONSTRAINT FK_TRACKER_Assignment FOREIGN KEY REFERENCES dbo.CUSTOMER_OFFER_ASSIGNMENT(AssignmentId),
    CustomerId              INT               NOT NULL CONSTRAINT FK_TRACKER_Customer FOREIGN KEY REFERENCES dbo.CUSTOMER_PROFILE_MIS(CustomerId),
    CampaignId              INT               NOT NULL CONSTRAINT FK_TRACKER_Campaign FOREIGN KEY REFERENCES dbo.CAMPAIGN_TEMPLATE(CampaignId),
    TargetAmount            DECIMAL(18,2)     NOT NULL,
    CurrentQualifiedSpend   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_TRACKER_CurrentSpend DEFAULT (0.00),
    RemainingAmount         DECIMAL(18,2)     NOT NULL,
    ProgressPercentage      DECIMAL(6,2)      NOT NULL CONSTRAINT DF_TRACKER_Pct DEFAULT (0.00),
    QualifiedTxCount        INT               NOT NULL CONSTRAINT DF_TRACKER_TxCount DEFAULT (0),
    ExcludedTxCount         INT               NOT NULL CONSTRAINT DF_TRACKER_ExclCount DEFAULT (0),
    IsCompleted             BIT               NOT NULL CONSTRAINT DF_TRACKER_IsCompleted DEFAULT (0),
    CompletedAt             DATETIME2(3)      NULL,
    LastTransactionAt       DATETIME2(3)      NULL,
    RowVersion              ROWVERSION        NOT NULL -- EF Core Optimistic Concurrency Token
);
CREATE INDEX IX_TRACKER_Customer_Campaign ON dbo.OFFER_PROGRESS_TRACKER(CustomerId, CampaignId);
GO

-- TABLO 8: REWARD_FULFILLMENT_QUEUE (Ödül Yükleme ve İade Kaynaklı Ödül Geri Alma / Clawback Kuyruğu)
CREATE TABLE dbo.REWARD_FULFILLMENT_QUEUE (
    FulfillmentId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_REWARD_FULFILLMENT_QUEUE PRIMARY KEY,
    TrackerId               INT               NOT NULL CONSTRAINT FK_FULFILLMENT_Tracker FOREIGN KEY REFERENCES dbo.OFFER_PROGRESS_TRACKER(TrackerId),
    CustomerId              INT               NOT NULL CONSTRAINT FK_FULFILLMENT_Customer FOREIGN KEY REFERENCES dbo.CUSTOMER_PROFILE_MIS(CustomerId),
    CampaignId              INT               NOT NULL CONSTRAINT FK_FULFILLMENT_Campaign FOREIGN KEY REFERENCES dbo.CAMPAIGN_TEMPLATE(CampaignId),
    RewardAmount            DECIMAL(18,2)     NOT NULL,
    RewardType              NVARCHAR(30)      NOT NULL, -- 'OdeaPuan', 'Cashback'
    Status                  NVARCHAR(40)      NOT NULL, -- 'PendingSettlement', 'Loaded', 'ClawedBack'
    ScheduledLoadDate       DATETIME2(3)      NOT NULL,
    LoadedAt                DATETIME2(3)      NULL,
    ClawedBackAt            DATETIME2(3)      NULL,
    ClawbackReason          NVARCHAR(300)     NULL,
    UpdatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_FULFILLMENT_UpdatedAt DEFAULT (SYSUTCDATETIME())
);
GO

/* =========================================================================================
   3. ENFLASYON-DÜZELTMELİ FEATURE STORE GÖRÜNÜMÜ (vw_CustomerCausalFeatures)
   ========================================================================================= */
CREATE VIEW dbo.vw_CustomerCausalFeatures
AS
WITH CustomerTxAgg AS (
    SELECT
        t.CustomerId,
        COUNT(CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN 1 END) AS ValidTxCount,
        ISNULL(SUM(CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN t.Amount ELSE 0 END), 0.00) AS TotalNominalSpend,
        COUNT(DISTINCT CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN t.ResolvedSector END) AS DistinctSectorCount,
        ISNULL(MAX(CASE WHEN t.ResolvedSector = N'Market & Gıda' AND t.IsReversalOrRefund = 0 THEN t.Amount ELSE 0 END), 0.00) AS MaxGroceryTxAmount,
        ISNULL(MAX(CASE WHEN t.ResolvedSector = N'E-Ticaret & Pazaryeri' AND t.IsReversalOrRefund = 0 THEN t.Amount ELSE 0 END), 0.00) AS MaxEcommerceTxAmount
    FROM dbo.TRANSACTION_LEDGER_FEED t
    GROUP BY t.CustomerId
)
SELECT
    c.CustomerId,
    c.CustomerExternalNo,
    c.FullName,
    c.PrivilegesSegment,
    c.TotalCreditLimit,
    c.AvailableCreditLimit,
    CAST(c.AvailableCreditLimit / NULLIF(c.TotalCreditLimit, 0) AS DECIMAL(6,4)) AS AvailableLimitRatio,
    c.MisMonthlyNetProfit,
    c.HistoricalBaselineSpend,
    c.RealBaselineSpendIndex,
    c.SpendElasticityPersona,
    c.IsDormant,
    c.DaysSinceLastTx,
    c.HasKvkkIysConsent,
    c.PreferredRewardType,
    c.BehavioralClusterId,
    ISNULL(agg.ValidTxCount, 0) AS ValidTxCount,
    ISNULL(agg.TotalNominalSpend, 0.00) AS RecentNominalSpend,
    ISNULL(agg.DistinctSectorCount, 0) AS DistinctSectorCount,
    CAST(PERCENT_RANK() OVER (ORDER BY c.RealBaselineSpendIndex) AS DECIMAL(6,4)) AS PortfolioRealSpendQuantile,
    CAST(NTILE(10) OVER (ORDER BY c.RealBaselineSpendIndex) AS INT) AS SpendDecile
FROM dbo.CUSTOMER_PROFILE_MIS c
LEFT JOIN CustomerTxAgg agg ON c.CustomerId = agg.CustomerId;
GO

/* =========================================================================================
   4. BAŞLANGIÇ (SEED) VERİLERİ
   ========================================================================================= */

-- 4.1 Sektörel Aylık Enflasyon Endeksi ve Deflatör Katsayıları (Son 6 Ay)
INSERT INTO dbo.INFLATION_INDEX_MONTHLY (YearMonth, SectorName, CpiIndexValue, DeflatorMultiplier)
VALUES
(202606, N'Genel',                 100.0000, 1.185000),
(202607, N'Genel',                 103.2000, 1.148256),
(202608, N'Genel',                 106.8000, 1.109551),
(202609, N'Genel',                 110.5000, 1.072398),
(202610, N'Genel',                 114.4000, 1.035839),
(202611, N'Genel',                 118.5000, 1.000000),
(202606, N'Market & Gıda',         100.0000, 1.210000),
(202611, N'Market & Gıda',         121.0000, 1.000000),
(202606, N'E-Ticaret & Pazaryeri', 100.0000, 1.170000),
(202611, N'E-Ticaret & Pazaryeri', 117.0000, 1.000000),
(202606, N'Akaryakıt',             100.0000, 1.160000),
(202611, N'Akaryakıt',             116.0000, 1.000000);

-- 4.2 Kampanya Şablonları (3 Ana Kampanya Kurgusu)
INSERT INTO dbo.CAMPAIGN_TEMPLATE (
    CampaignCode, CampaignName, CampaignType, TargetSector, DefaultRewardType,
    TotalBudget, ConsumedBudget, MaxRewardPerCustomer, MinMisProfitThreshold,
    InterchangeMarginRate, StartDate, EndDate, IsActive, KillSwitchTriggered
)
VALUES
(N'ODEA-2026-UPSELL-01', N'Kişiye Özel Toplam Harcama Artışı (Nedensel Uplift) Kampanyası', N'TotalSpendUpsell',   N'Tümü',                  N'OdeaPuan', 2500000.00, 185000.00, 2500.00, -250.00, 0.0260, '2026-11-01', '2026-12-31', 1, 0),
(N'ODEA-2026-SECTOR-02', N'E-Ticaret ve Market Sektörel Çapraz Harcama Kampanyası',        N'SectoralCrossSell',  N'E-Ticaret & Pazaryeri', N'Cashback', 1200000.00,  92000.00, 1500.00,    0.00, 0.0280, '2026-11-01', '2026-12-31', 1, 0),
(N'ODEA-2026-WAKEUP-03', N'İnaktif (Uyuyan) Kredi Kartı Canlandırma Kampanyası',           N'DormantReactivation',N'Tümü',                  N'Cashback',  800000.00,  45000.00, 1000.00, -500.00, 0.0250, '2026-11-01', '2026-12-31', 1, 0);

-- 4.3 3 Katmanlı POS İşlem Zenginleştirme Önbelleği (MERCHANT_MASTER_CACHE)
INSERT INTO dbo.MERCHANT_MASTER_CACHE (
    RawMerchantKey, RawMcc_DE18, CleanedBrandName, ResolvedSector,
    SubMerchantProvider, ClassificationLayer, ConfidenceScore, IsCashLikeOrExcludedMcc
)
VALUES
(N'MIGROS TICARET A.S. ISTANBUL TR',       N'5411', N'MİGROS',        N'Market & Gıda',         NULL,      1, 0.9990, 0),
(N'CARREFOURSA HIPERMARKET ATASEHIR',      N'5411', N'CARREFOURSA',   N'Market & Gıda',         NULL,      1, 0.9980, 0),
(N'SHELL & TURCAS PETROL MASLAK',          N'5541', N'SHELL',         N'Akaryakıt',             NULL,      1, 0.9990, 0),
(N'OPET PETROLCULUK KADIKOY',              N'5541', N'OPET',          N'Akaryakıt',             NULL,      1, 0.9990, 0),
(N'IYZICO*TRENDYOL.COM ISTANBUL',          N'5399', N'TRENDYOL',      N'E-Ticaret & Pazaryeri', N'IYZICO', 2, 0.9750, 0),
(N'PARAM*GETIR PERAKENDE LOJISTIK',        N'7399', N'GETİR',         N'Market & Gıda',         N'PARAM',  2, 0.9680, 0),
(N'PAYTR*HEPSIBURADA D-MARKET',            N'5399', N'HEPSİBURADA',   N'E-Ticaret & Pazaryeri', N'PAYTR',  2, 0.9790, 0),
(N'SIPAY*ZARA TURKIYE GIYIM',              N'5651', N'ZARA',          N'Giyim & Tekstil',       N'SIPAY',  2, 0.9640, 0),
(N'MOKA*YEMEKSEPETI BANABI',               N'5812', N'YEMEKSEPETİ',   N'Restoran & Kafe',       N'MOKA',   2, 0.9610, 0),
(N'BEREKET UNLU MAMULLERI VE GIDA SAN',    N'5999', N'BEREKET GIDA',  N'Market & Gıda',         NULL,      3, 0.9340, 0),
(N'ALTINBAS KUYUMCULUK KAPALICARSI',       N'6051', N'ALTINBAŞ',      N'Nakit Benzeri / Yasaklı',NULL,     1, 0.9950, 1),
(N'BILYONER INTERAKTIF HIZMETLER',         N'7995', N'BİLYONER',      N'Nakit Benzeri / Yasaklı',NULL,     1, 0.9990, 1);

-- 4.4 Temsili Müşteri Portföyü (Persuadable, Free-Rider, Dormant, Ayrıcalıklar Dünyası Segmentleri)
INSERT INTO dbo.CUSTOMER_PROFILE_MIS (
    CustomerExternalNo, FullName, CardNumberMasked, PrivilegesSegment,
    TotalCreditLimit, AvailableCreditLimit, MisMonthlyNetProfit,
    HistoricalBaselineSpend, RealBaselineSpendIndex, SpendElasticityPersona,
    IsDormant, DaysSinceLastTx, HasKvkkIysConsent, PreferredRewardType, BehavioralClusterId
)
VALUES
-- 1001: Yüksek Uplift / Persuadable (Ayrıcalıklar Dünyası Altın)
(N'ODEA-CUST-1001', N'Ahmet Yılmaz',   N'5400 61** **** 1001', N'Ayrıcalıklar Dünyası Altın',  150000.00, 110000.00,  4250.00, 24000.00, 25800.00, N'Persuadable',         0,  2, 1, N'OdeaPuan', 1),
-- 1002: Yüksek Uplift / Persuadable (Ayrıcalıklar Dünyası Gümüş - Nakit İade Seven)
(N'ODEA-CUST-1002', N'Zeynep Kaya',    N'5400 61** **** 1002', N'Ayrıcalıklar Dünyası Gümüş',   85000.00,  62000.00,  2150.00, 14500.00, 15400.00, N'Persuadable',         0,  4, 1, N'Cashback', 2),
-- 1003: Free-Rider / Sure Thing (Kampanya olmasa da ayda 65.000 TL harcayan organik müşteri)
(N'ODEA-CUST-1003', N'Mehmet Demir',   N'5400 61** **** 1003', N'Ayrıcalıklar Dünyası Özel',   300000.00, 210000.00,  8900.00, 65000.00, 68200.00, N'FreeRider_SureThing', 0,  1, 1, N'OdeaPuan', 3),
-- 1004: İnaktif / Uyuyan Kart (Son 115 gündür işlem yok, yüksek MIS mevduat kârlılığı var)
(N'ODEA-CUST-1004', N'Elif Şahin',     N'5400 61** **** 1004', N'Ayrıcalıklar Dünyası Altın',  120000.00, 120000.00,  5400.00,   450.00,   520.00, N'Dormant_Sleeper',     1,115, 1, N'Cashback', 4),
-- 1005: Persuadable Sektörel Geçiş Adayı (Standart Segment)
(N'ODEA-CUST-1005', N'Caner Öztürk',   N'5400 61** **** 1005', N'Standart',                     50000.00,  34000.00,   980.00,  9500.00, 10100.00, N'Persuadable',         0,  5, 1, N'OdeaPuan', 2),
-- 1006: Boş Limiti Düşük Müşteri (MILP Boş Limit Kısıtı Testi İçin)
(N'ODEA-CUST-1006', N'Selin Arslan',   N'5400 61** **** 1006', N'Standart',                     30000.00,   4500.00,   620.00,  8000.00,  8400.00, N'Persuadable',         0,  3, 1, N'Cashback', 1),
-- 1007: Kontrol Grubu (Stratified Hold-out RCT & CUPED Karşılaştırması İçin)
(N'ODEA-CUST-1007', N'Burak Çelik',    N'5400 61** **** 1007', N'Ayrıcalıklar Dünyası Altın',  140000.00,  98000.00,  3900.00, 23500.00, 25100.00, N'Persuadable',         0,  2, 1, N'OdeaPuan', 1),
-- 1008: İnaktif / Uyuyan Kart 2
(N'ODEA-CUST-1008', N'Ayşe Yıldız',    N'5400 61** **** 1008', N'Ayrıcalıklar Dünyası Gümüş',   75000.00,  75000.00,  1850.00,     0.00,     0.00, N'Dormant_Sleeper',     1,140, 1, N'OdeaPuan', 4);

-- 4.5 R-Learner + MILP Tarafından Atanmış Örnek Teklifler (CUSTOMER_OFFER_ASSIGNMENT)
INSERT INTO dbo.CUSTOMER_OFFER_ASSIGNMENT (
    CampaignId, CustomerId, MicroBucketId, MicroBucketName, ExperimentGroup,
    ExpectedOrganicSpend, PredictedUpliftSpend, PropensityScore, ReactivationProbability,
    AssignedTargetAmount, AssignedRewardAmount, RewardType, ExpectedNetIncrementalProfit,
    XaiReasonText, XaiTopFeaturesJson, AlternativeOffersJson, CustomerSelectedOfferIndex
)
VALUES
-- Müşteri 1001 (Ahmet Yılmaz): Organik beklenti 25.800 TL, MILP Hedefi 36.000 TL (+10.200 TL Uplift), Ödül 750 TL OdeaPuan
(1, 1001, 14, N'Kova-14: Altın Segment Yüksek Uplift (36.000 TL Hedef / 750 TL Ödül)', N'Treatment',
 25800.00, 11400.00, 0.5800, 0.0000, 36000.00, 750.00, N'OdeaPuan', 211.40,
 N'Son 3 aydaki düzenli Market ve E-Ticaret harcama ivmeniz (+%18 reel artış), Ayrıcalıklar Dünyası Altın segment statünüz ve 110.000 TL kullanılabilir limitiniz dikkate alınarak 36.000 TL harcama hedefine 750 TL OdeaPuan teklifi oluşturulmuştur.',
 N'[{"Feature":"RealBaselineSpendIndex","Impact":0.42},{"Feature":"PrivilegesSegment_Altin","Impact":0.31},{"Feature":"EcommerceSpendTrend","Impact":0.27}]',
 N'[{"Index":1,"Title":"36.000 TL Harcamaya 750 TL OdeaPuan","Target":36000,"Reward":750,"RewardType":"OdeaPuan"},{"Index":2,"Title":"38.500 TL Harcamaya 700 TL Nakit İade","Target":38500,"Reward":700,"RewardType":"Cashback"},{"Index":3,"Title":"E-Ticarette 15.000 TL Harcamaya 800 TL OdeaPuan","Target":15000,"Reward":800,"RewardType":"OdeaPuan"}]',
 1),

-- Müşteri 1002 (Zeynep Kaya): Organik beklenti 15.400 TL, MILP Hedefi 22.000 TL, Ödül 450 TL Cashback
(2, 1002, 9, N'Kova-09: Gümüş Segment Sektörel Geçiş (22.000 TL Hedef / 450 TL Nakit İade)', N'Treatment',
 15400.00, 7200.00, 0.6200, 0.0000, 22000.00, 450.00, N'Cashback', 151.60,
 N'Ödeme kuruluşları (İyzico/PayTR) üzerinden gerçekleştirdiğiniz E-Ticaret alışveriş sıklığınız ve Nakit İade (Cashback) tercihiniz doğrultusunda 22.000 TL hedefe 450 TL Nakit İade atanmıştır.',
 N'[{"Feature":"EcommerceTxFrequency","Impact":0.46},{"Feature":"PreferredReward_Cashback","Impact":0.34},{"Feature":"AvailableLimitRatio","Impact":0.20}]',
 N'[{"Index":1,"Title":"22.000 TL Harcamaya 450 TL Nakit İade","Target":22000,"Reward":450,"RewardType":"Cashback"},{"Index":2,"Title":"20.000 TL Harcamaya 500 TL OdeaPuan","Target":20000,"Reward":500,"RewardType":"OdeaPuan"},{"Index":3,"Title":"25.000 TL Harcamaya 550 TL Nakit İade","Target":25000,"Reward":550,"RewardType":"Cashback"}]',
 1),

-- Müşteri 1004 (Elif Şahin - Uyuyan Kart): Uyandırma Hedefi 5.000 TL, Ödül 400 TL Cashback
(3, 1004, 2, N'Kova-02: İnaktif Kart Uyandırma (5.000 TL İlk Harcamaya 400 TL Nakit İade)', N'Treatment',
 520.00, 6800.00, 0.4500, 0.1450, 5000.00, 400.00, N'Cashback', 135.00,
 N'Kartınızın 115 gündür inaktif olması ve Ayrıcalıklar Dünyası Altın segmentindeki yüksek banka kârlılığınız nedeniyle yeniden aktifleşmeye özel 5.000 TL harcamaya 400 TL Nakit İade tanımlanmıştır.',
 N'[{"Feature":"DaysSinceLastTx_115","Impact":0.52},{"Feature":"MisMonthlyNetProfit","Impact":0.30},{"Feature":"ReactivationElasticity","Impact":0.18}]',
 N'[{"Index":1,"Title":"5.000 TL Harcamaya 400 TL Nakit İade","Target":5000,"Reward":400,"RewardType":"Cashback"},{"Index":2,"Title":"4.000 TL Harcamaya 450 TL OdeaPuan","Target":4000,"Reward":450,"RewardType":"OdeaPuan"},{"Index":3,"Title":"Markette 3.500 TL Harcamaya 350 TL Nakit İade","Target":3500,"Reward":350,"RewardType":"Cashback"}]',
 1),

-- Müşteri 1007 (Burak Çelik - Kontrol Grubu / Hold-out RCT)
(1, 1007, 0, N'Kova-00: Tabakalı Kontrol Grubu (Hold-out RCT)', N'Control_Holdout',
 25100.00, 0.00, 0.5000, 0.0000, 0.00, 0.00, N'OdeaPuan', 0.00,
 N'CUPED ve Tabakalı Rastgeleleştirilmiş Kontrol Grubu (Stratified RCT) net ekonomik katkı ölçümü için kontrol grubunda tutulmuştur.',
 N'[{"Feature":"StratifiedHoldoutGroup","Impact":1.00}]',
 N'[]',
 1);

-- 4.6 Aktif Sayaç Kayıtları (OFFER_PROGRESS_TRACKER)
INSERT INTO dbo.OFFER_PROGRESS_TRACKER (
    AssignmentId, CustomerId, CampaignId, TargetAmount,
    CurrentQualifiedSpend, RemainingAmount, ProgressPercentage,
    QualifiedTxCount, ExcludedTxCount, IsCompleted, CompletedAt, LastTransactionAt
)
VALUES
(1, 1001, 1, 36000.00, 21500.00, 14500.00, 59.72, 4, 2, 0, NULL, DATEADD(HOUR, -3, SYSUTCDATETIME())),
(2, 1002, 2, 22000.00, 18000.00,  4000.00, 81.82, 3, 0, 0, NULL, DATEADD(HOUR, -5, SYSUTCDATETIME())),
(3, 1004, 3,  5000.00,  5200.00,     0.00,100.00, 2, 0, 1, DATEADD(HOUR, -1, SYSUTCDATETIME()), DATEADD(HOUR, -1, SYSUTCDATETIME()));

-- 4.7 Örnek Kart İşlemleri ve Anti-Gaming Denetim Kayıtları (TRANSACTION_LEDGER_FEED)
INSERT INTO dbo.TRANSACTION_LEDGER_FEED (
    AuthorizationCode, CustomerId, TransactionTimestamp, Amount, Currency,
    RawMcc_DE18, RawMerchantName_DE43, ResolvedBrand, ResolvedSector,
    ClassificationLayer, IsReversalOrRefund, OriginalTransactionId,
    IsExcludedByAntiGaming, ExclusionReason, ProcessingLatencyMs
)
VALUES
-- Ahmet Yılmaz (1001) Geçerli İşlemler
(N'AUTH-9001', 1001, DATEADD(DAY, -5, SYSUTCDATETIME()), 6500.00, N'TRY', N'5411', N'MIGROS TICARET A.S. ISTANBUL TR',    N'MİGROS',      N'Market & Gıda',         1, 0, NULL, 0, NULL, 8.40),
(N'AUTH-9002', 1001, DATEADD(DAY, -4, SYSUTCDATETIME()), 8200.00, N'TRY', N'5399', N'IYZICO*TRENDYOL.COM ISTANBUL',       N'TRENDYOL',    N'E-Ticaret & Pazaryeri', 2, 0, NULL, 0, NULL, 14.20),
(N'AUTH-9003', 1001, DATEADD(DAY, -2, SYSUTCDATETIME()), 3800.00, N'TRY', N'5541', N'SHELL & TURCAS PETROL MASLAK',       N'SHELL',       N'Akaryakıt',             1, 0, NULL, 0, NULL, 7.10),
(N'AUTH-9004', 1001, DATEADD(HOUR,-4, SYSUTCDATETIME()), 3000.00, N'TRY', N'7399', N'PARAM*GETIR PERAKENDE LOJISTIK',     N'GETİR',       N'Market & Gıda',         2, 0, NULL, 0, NULL, 11.80),
-- Ahmet Yılmaz (1001) Anti-Gaming 1: 30 Dk İçinde Aynı İşyerinden Bölünmüş İşlem (Split-Transaction)
(N'AUTH-9005', 1001, DATEADD(MINUTE,-225,SYSUTCDATETIME()),2500.00,N'TRY',N'7399', N'PARAM*GETIR PERAKENDE LOJISTIK',     N'GETİR',       N'Market & Gıda',         2, 0, NULL, 1, N'Anti-Gaming: Aynı üye işyerinde 30 dakika içinde tespit edilen bölünmüş işlem (Split-Transaction).', 9.60),
-- Ahmet Yılmaz (1001) Anti-Gaming 2: Nakit Benzeri / Yasaklı MCC (6051 Kuyum/Altın)
(N'AUTH-9006', 1001, DATEADD(DAY, -1, SYSUTCDATETIME()),12000.00, N'TRY', N'6051', N'ALTINBAS KUYUMCULUK KAPALICARSI',    N'ALTINBAŞ',    N'Nakit Benzeri / Yasaklı',1, 0, NULL, 1, N'Anti-Gaming: Nakit benzeri / kampanya dışı MCC kodu (6051).', 6.30),
-- Zeynep Kaya (1002) Geçerli İşlemler
(N'AUTH-9007', 1002, DATEADD(DAY, -3, SYSUTCDATETIME()),11000.00, N'TRY', N'5399', N'PAYTR*HEPSIBURADA D-MARKET',         N'HEPSİBURADA', N'E-Ticaret & Pazaryeri', 2, 0, NULL, 0, NULL, 15.10),
(N'AUTH-9008', 1002, DATEADD(HOUR,-5, SYSUTCDATETIME()), 7000.00, N'TRY', N'5651', N'SIPAY*ZARA TURKIYE GIYIM',           N'ZARA',        N'Giyim & Tekstil',       2, 0, NULL, 0, NULL, 13.40),
-- Elif Şahin (1004 - Uyuyan Kart Hedefini Tamamlamış)
(N'AUTH-9009', 1004, DATEADD(HOUR,-1, SYSUTCDATETIME()), 5200.00, N'TRY', N'5399', N'IYZICO*TRENDYOL.COM ISTANBUL',       N'TRENDYOL',    N'E-Ticaret & Pazaryeri', 2, 0, NULL, 0, NULL, 12.90);

-- 4.8 Ödül Kuyruğu (REWARD_FULFILLMENT_QUEUE)
INSERT INTO dbo.REWARD_FULFILLMENT_QUEUE (
    TrackerId, CustomerId, CampaignId, RewardAmount, RewardType, Status, ScheduledLoadDate
)
VALUES
(3, 1004, 3, 400.00, N'Cashback', N'PendingSettlement', DATEADD(DAY, 1, SYSUTCDATETIME()));
GO

PRINT N'ODEA-028 veritabanı, 8 tablo, indeksler, vw_CustomerCausalFeatures görünümü ve başlangıç verileri başarıyla oluşturuldu.';
GO
