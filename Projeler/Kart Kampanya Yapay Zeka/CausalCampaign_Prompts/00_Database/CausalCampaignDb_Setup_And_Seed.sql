/* =========================================================================================
   SOLUTION   : CausalCampaign.sln
   DATABASE   : DB_Odeabank_DVP_Cards (Existing Database - NO CREATE DATABASE)
   PREFIX     : CasualCampaign_
   FILE       : CausalCampaignDb_Setup_And_Seed.sql
   DESCRIPTION: Creates all 8 Clean Architecture core tables prefixed with CasualCampaign_,
                performance indexes, the inflation-adjusted Feature Store view
                (dbo.vw_CasualCampaign_CustomerCausalFeatures), and initial seed data
                inside the existing DB_Odeabank_DVP_Cards database.
   ========================================================================================= */

SET NOCOUNT ON;
GO

-- Drop existing prefixed View and prefixed Tables in dependency order (Idempotent execution)
IF OBJECT_ID(N'dbo.vw_CasualCampaign_CustomerCausalFeatures', N'V') IS NOT NULL DROP VIEW dbo.vw_CasualCampaign_CustomerCausalFeatures;
IF OBJECT_ID(N'dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE;
IF OBJECT_ID(N'dbo.CasualCampaign_OFFER_PROGRESS_TRACKER', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_OFFER_PROGRESS_TRACKER;
IF OBJECT_ID(N'dbo.CasualCampaign_TRANSACTION_LEDGER_FEED', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_TRANSACTION_LEDGER_FEED;
IF OBJECT_ID(N'dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT;
IF OBJECT_ID(N'dbo.CasualCampaign_MERCHANT_MASTER_CACHE', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_MERCHANT_MASTER_CACHE;
IF OBJECT_ID(N'dbo.CasualCampaign_INFLATION_INDEX_MONTHLY', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_INFLATION_INDEX_MONTHLY;
IF OBJECT_ID(N'dbo.CasualCampaign_CUSTOMER_PROFILE_MIS', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_CUSTOMER_PROFILE_MIS;
IF OBJECT_ID(N'dbo.CasualCampaign_CAMPAIGN_TEMPLATE', N'U') IS NOT NULL DROP TABLE dbo.CasualCampaign_CAMPAIGN_TEMPLATE;
GO

/* =========================================================================================
   2. CORE TABLES (PREFIXED WITH CasualCampaign_ - EF CORE 10.0 COMPATIBLE)
   ========================================================================================= */

-- TABLE 1: CasualCampaign_CAMPAIGN_TEMPLATE
CREATE TABLE dbo.CasualCampaign_CAMPAIGN_TEMPLATE (
    CampaignId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CasualCampaign_CAMPAIGN_TEMPLATE PRIMARY KEY,
    CampaignCode            NVARCHAR(30)      NOT NULL CONSTRAINT UQ_CasualCampaign_CAMPAIGN_Code UNIQUE,
    CampaignName            NVARCHAR(200)     NOT NULL,
    CampaignType            NVARCHAR(40)      NOT NULL, -- 'TotalSpendUpsell', 'SectoralCrossSell', 'DormantReactivation'
    TargetSector            NVARCHAR(80)      NOT NULL, -- 'All', 'Grocery & Supermarket', 'Fuel', 'E-Commerce & Marketplace', 'Apparel & Fashion'
    DefaultRewardType       NVARCHAR(30)      NOT NULL, -- 'RewardPoints', 'Cashback'
    TotalBudget             DECIMAL(18,2)     NOT NULL,
    ConsumedBudget          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_Consumed DEFAULT (0.00),
    MaxRewardPerCustomer    DECIMAL(18,2)     NOT NULL,
    MinMisProfitThreshold   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_MinMis DEFAULT (-500.00),
    InterchangeMarginRate   DECIMAL(9,4)      NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_Margin DEFAULT (0.0250),
    StartDate               DATETIME2(3)      NOT NULL,
    EndDate                 DATETIME2(3)      NOT NULL,
    IsActive                BIT               NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_IsActive DEFAULT (1),
    KillSwitchTriggered     BIT               NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_KillSwitch DEFAULT (0),
    KillSwitchReason        NVARCHAR(300)     NULL,
    CreatedBy               NVARCHAR(100)     NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_CreatedBy DEFAULT (N'SYSTEM_ARCHITECT'),
    CreatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_CasualCampaign_CAMPAIGN_CreatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion              ROWVERSION        NOT NULL
);
GO

-- TABLE 2: CasualCampaign_CUSTOMER_PROFILE_MIS
CREATE TABLE dbo.CasualCampaign_CUSTOMER_PROFILE_MIS (
    CustomerId              INT IDENTITY(1001,1) NOT NULL CONSTRAINT PK_CasualCampaign_CUSTOMER_PROFILE_MIS PRIMARY KEY,
    CustomerExternalNo      NVARCHAR(30)         NOT NULL CONSTRAINT UQ_CasualCampaign_CUSTOMER_ExternalNo UNIQUE,
    FullName                NVARCHAR(120)        NOT NULL,
    CardNumberMasked        NVARCHAR(25)         NOT NULL,
    PrestigeSegment         NVARCHAR(50)         NOT NULL, -- 'Standard', 'Silver', 'Gold', 'Private'
    TotalCreditLimit        DECIMAL(18,2)        NOT NULL,
    AvailableCreditLimit    DECIMAL(18,2)        NOT NULL,
    MisMonthlyNetProfit     DECIMAL(18,2)        NOT NULL,
    HistoricalBaselineSpend DECIMAL(18,2)        NOT NULL,
    RealBaselineSpendIndex  DECIMAL(18,2)        NOT NULL,
    SpendElasticityPersona  NVARCHAR(40)         NOT NULL, -- 'Persuadable', 'FreeRider_SureThing', 'Dormant_Sleeper', 'LostCause'
    IsDormant               BIT                  NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_IsDormant DEFAULT (0),
    DaysSinceLastTx         INT                  NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_DaysSinceLastTx DEFAULT (0),
    HasMarketingConsent     BIT                  NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_Consent DEFAULT (1),
    PreferredRewardType     NVARCHAR(30)         NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_PrefReward DEFAULT (N'RewardPoints'),
    BehavioralClusterId     INT                  NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_Cluster DEFAULT (1),
    CreatedAt               DATETIME2(3)         NOT NULL CONSTRAINT DF_CasualCampaign_CUSTOMER_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- TABLE 3: CasualCampaign_INFLATION_INDEX_MONTHLY
CREATE TABLE dbo.CasualCampaign_INFLATION_INDEX_MONTHLY (
    InflationIndexId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CasualCampaign_INFLATION_INDEX_MONTHLY PRIMARY KEY,
    YearMonth               INT               NOT NULL, -- e.g. 202606..202611
    SectorName              NVARCHAR(80)      NOT NULL,
    CpiIndexValue           DECIMAL(12,4)     NOT NULL,
    DeflatorMultiplier      DECIMAL(12,6)     NOT NULL,
    CONSTRAINT UQ_CasualCampaign_INFLATION_YearMonth_Sector UNIQUE (YearMonth, SectorName)
);
GO

-- TABLE 4: CasualCampaign_MERCHANT_MASTER_CACHE
CREATE TABLE dbo.CasualCampaign_MERCHANT_MASTER_CACHE (
    RawMerchantKey          NVARCHAR(250)     NOT NULL CONSTRAINT PK_CasualCampaign_MERCHANT_MASTER_CACHE PRIMARY KEY,
    RawMcc_DE18             NVARCHAR(10)      NOT NULL,
    CleanedBrandName        NVARCHAR(120)     NOT NULL,
    ResolvedSector          NVARCHAR(80)      NOT NULL,
    SubMerchantProvider     NVARCHAR(60)      NULL,     -- 'IYZICO', 'PARAM', 'PAYTR', 'SIPAY', 'MOKA', NULL
    ClassificationLayer     TINYINT           NOT NULL, -- 1=Dictionary, 2=PaymentFacilitatorRegex, 3=MachineLearning
    ConfidenceScore         DECIMAL(6,4)      NOT NULL,
    IsCashLikeOrExcludedMcc BIT               NOT NULL CONSTRAINT DF_CasualCampaign_MERCHANT_IsExcluded DEFAULT (0),
    UpdatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_CasualCampaign_MERCHANT_UpdatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- TABLE 5: CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT
CREATE TABLE dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT (
    AssignmentId                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT PRIMARY KEY,
    CampaignId                    INT               NOT NULL CONSTRAINT FK_CasualCampaign_ASSIGNMENT_Campaign FOREIGN KEY REFERENCES dbo.CasualCampaign_CAMPAIGN_TEMPLATE(CampaignId),
    CustomerId                    INT               NOT NULL CONSTRAINT FK_CasualCampaign_ASSIGNMENT_Customer FOREIGN KEY REFERENCES dbo.CasualCampaign_CUSTOMER_PROFILE_MIS(CustomerId),
    MicroBucketId                 INT               NOT NULL, -- 0..20
    MicroBucketName               NVARCHAR(100)     NOT NULL,
    ExperimentGroup               NVARCHAR(30)      NOT NULL, -- 'Treatment', 'Control_Holdout'
    ExpectedOrganicSpend          DECIMAL(18,2)     NOT NULL,
    PredictedUpliftSpend          DECIMAL(18,2)     NOT NULL,
    PropensityScore               DECIMAL(6,4)      NOT NULL,
    ReactivationProbability       DECIMAL(6,4)      NOT NULL CONSTRAINT DF_CasualCampaign_ASSIGNMENT_ReactProb DEFAULT (0.0000),
    AssignedTargetAmount          DECIMAL(18,2)     NOT NULL,
    AssignedRewardAmount          DECIMAL(18,2)     NOT NULL,
    RewardType                    NVARCHAR(30)      NOT NULL, -- 'RewardPoints', 'Cashback'
    ExpectedNetIncrementalProfit  DECIMAL(18,2)     NOT NULL,
    XaiReasonText                 NVARCHAR(500)     NOT NULL,
    XaiTopFeaturesJson            NVARCHAR(1000)    NOT NULL,
    AlternativeOffersJson         NVARCHAR(2000)    NOT NULL,
    CustomerSelectedOfferIndex    TINYINT           NOT NULL CONSTRAINT DF_CasualCampaign_ASSIGNMENT_SelectedIdx DEFAULT (1),
    IsApprovedByHuman             BIT               NOT NULL CONSTRAINT DF_CasualCampaign_ASSIGNMENT_Approved DEFAULT (1),
    AssignedAt                    DATETIME2(3)      NOT NULL CONSTRAINT DF_CasualCampaign_ASSIGNMENT_AssignedAt DEFAULT (SYSUTCDATETIME())
);
CREATE INDEX IX_CasualCampaign_ASSIGNMENT_Customer_Campaign ON dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT(CustomerId, CampaignId);
GO

-- TABLE 6: CasualCampaign_TRANSACTION_LEDGER_FEED
CREATE TABLE dbo.CasualCampaign_TRANSACTION_LEDGER_FEED (
    TransactionId           BIGINT IDENTITY(100001,1) NOT NULL CONSTRAINT PK_CasualCampaign_TRANSACTION_LEDGER_FEED PRIMARY KEY,
    AuthorizationCode       NVARCHAR(20)              NOT NULL,
    CustomerId              INT                       NOT NULL CONSTRAINT FK_CasualCampaign_TX_Customer FOREIGN KEY REFERENCES dbo.CasualCampaign_CUSTOMER_PROFILE_MIS(CustomerId),
    TransactionTimestamp    DATETIME2(3)              NOT NULL,
    Amount                  DECIMAL(18,2)             NOT NULL,
    Currency                NVARCHAR(10)              NOT NULL CONSTRAINT DF_CasualCampaign_TX_Currency DEFAULT (N'TRY'),
    RawMcc_DE18             NVARCHAR(10)              NOT NULL,
    RawMerchantName_DE43    NVARCHAR(250)             NOT NULL,
    ResolvedBrand           NVARCHAR(120)             NOT NULL,
    ResolvedSector          NVARCHAR(80)              NOT NULL,
    ClassificationLayer     TINYINT                   NOT NULL CONSTRAINT DF_CasualCampaign_TX_Layer DEFAULT (1),
    IsReversalOrRefund      BIT                       NOT NULL CONSTRAINT DF_CasualCampaign_TX_IsRefund DEFAULT (0),
    OriginalTransactionId   BIGINT                    NULL,
    IsExcludedByAntiGaming  BIT                       NOT NULL CONSTRAINT DF_CasualCampaign_TX_IsExcluded DEFAULT (0),
    ExclusionReason         NVARCHAR(250)             NULL,
    ProcessingLatencyMs     DECIMAL(10,2)             NOT NULL CONSTRAINT DF_CasualCampaign_TX_Latency DEFAULT (12.50)
);
CREATE INDEX IX_CasualCampaign_TX_Customer_Timestamp ON dbo.CasualCampaign_TRANSACTION_LEDGER_FEED(CustomerId, TransactionTimestamp DESC);
CREATE INDEX IX_CasualCampaign_TX_Idempotency ON dbo.CasualCampaign_TRANSACTION_LEDGER_FEED(CustomerId, AuthorizationCode, IsReversalOrRefund);
CREATE INDEX IX_CasualCampaign_TX_SplitWindow ON dbo.CasualCampaign_TRANSACTION_LEDGER_FEED(CustomerId, ResolvedBrand, TransactionTimestamp DESC);
GO

-- TABLE 7: CasualCampaign_OFFER_PROGRESS_TRACKER
CREATE TABLE dbo.CasualCampaign_OFFER_PROGRESS_TRACKER (
    TrackerId               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CasualCampaign_OFFER_PROGRESS_TRACKER PRIMARY KEY,
    AssignmentId            INT               NOT NULL CONSTRAINT UQ_CasualCampaign_TRACKER_Assignment UNIQUE CONSTRAINT FK_CasualCampaign_TRACKER_Assignment FOREIGN KEY REFERENCES dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT(AssignmentId),
    CustomerId              INT               NOT NULL CONSTRAINT FK_CasualCampaign_TRACKER_Customer FOREIGN KEY REFERENCES dbo.CasualCampaign_CUSTOMER_PROFILE_MIS(CustomerId),
    CampaignId              INT               NOT NULL CONSTRAINT FK_CasualCampaign_TRACKER_Campaign FOREIGN KEY REFERENCES dbo.CasualCampaign_CAMPAIGN_TEMPLATE(CampaignId),
    TargetAmount            DECIMAL(18,2)     NOT NULL,
    CurrentQualifiedSpend   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_CasualCampaign_TRACKER_CurrentSpend DEFAULT (0.00),
    RemainingAmount         DECIMAL(18,2)     NOT NULL,
    ProgressPercentage      DECIMAL(6,2)      NOT NULL CONSTRAINT DF_CasualCampaign_TRACKER_Pct DEFAULT (0.00),
    QualifiedTxCount        INT               NOT NULL CONSTRAINT DF_CasualCampaign_TRACKER_TxCount DEFAULT (0),
    ExcludedTxCount         INT               NOT NULL CONSTRAINT DF_CasualCampaign_TRACKER_ExclCount DEFAULT (0),
    IsCompleted             BIT               NOT NULL CONSTRAINT DF_CasualCampaign_TRACKER_IsCompleted DEFAULT (0),
    CompletedAt             DATETIME2(3)      NULL,
    LastTransactionAt       DATETIME2(3)      NULL,
    RowVersion              ROWVERSION        NOT NULL
);
CREATE INDEX IX_CasualCampaign_TRACKER_Customer_Campaign ON dbo.CasualCampaign_OFFER_PROGRESS_TRACKER(CustomerId, CampaignId);
GO

-- TABLE 8: CasualCampaign_REWARD_FULFILLMENT_QUEUE
CREATE TABLE dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE (
    FulfillmentId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CasualCampaign_REWARD_FULFILLMENT_QUEUE PRIMARY KEY,
    TrackerId               INT               NOT NULL CONSTRAINT FK_CasualCampaign_FULFILLMENT_Tracker FOREIGN KEY REFERENCES dbo.CasualCampaign_OFFER_PROGRESS_TRACKER(TrackerId),
    CustomerId              INT               NOT NULL CONSTRAINT FK_CasualCampaign_FULFILLMENT_Customer FOREIGN KEY REFERENCES dbo.CasualCampaign_CUSTOMER_PROFILE_MIS(CustomerId),
    CampaignId              INT               NOT NULL CONSTRAINT FK_CasualCampaign_FULFILLMENT_Campaign FOREIGN KEY REFERENCES dbo.CasualCampaign_CAMPAIGN_TEMPLATE(CampaignId),
    RewardAmount            DECIMAL(18,2)     NOT NULL,
    RewardType              NVARCHAR(30)      NOT NULL, -- 'RewardPoints', 'Cashback'
    Status                  NVARCHAR(40)      NOT NULL, -- 'PendingSettlement', 'Loaded', 'ClawedBack'
    ScheduledLoadDate       DATETIME2(3)      NOT NULL,
    LoadedAt                DATETIME2(3)      NULL,
    ClawedBackAt            DATETIME2(3)      NULL,
    ClawbackReason          NVARCHAR(300)     NULL,
    UpdatedAt               DATETIME2(3)      NOT NULL CONSTRAINT DF_CasualCampaign_FULFILLMENT_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion              ROWVERSION        NOT NULL
);
CREATE INDEX IX_CasualCampaign_FULFILLMENT_Tracker_Status ON dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE(TrackerId, Status);
GO

/* =========================================================================================
   3. INFLATION-ADJUSTED FEATURE STORE VIEW (dbo.vw_CasualCampaign_CustomerCausalFeatures)
   ========================================================================================= */
CREATE VIEW dbo.vw_CasualCampaign_CustomerCausalFeatures
AS
WITH CustomerTxAgg AS (
    SELECT
        t.CustomerId,
        COUNT(CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN 1 END) AS ValidTxCount,
        ISNULL(SUM(CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN t.Amount ELSE 0 END), 0.00) AS TotalNominalSpend,
        COUNT(DISTINCT CASE WHEN t.IsReversalOrRefund = 0 AND t.IsExcludedByAntiGaming = 0 THEN t.ResolvedSector END) AS DistinctSectorCount,
        ISNULL(MAX(CASE WHEN t.ResolvedSector = N'Grocery & Supermarket' AND t.IsReversalOrRefund = 0 THEN t.Amount ELSE 0 END), 0.00) AS MaxGroceryTxAmount,
        ISNULL(MAX(CASE WHEN t.ResolvedSector = N'E-Commerce & Marketplace' AND t.IsReversalOrRefund = 0 THEN t.Amount ELSE 0 END), 0.00) AS MaxEcommerceTxAmount
    FROM dbo.CasualCampaign_TRANSACTION_LEDGER_FEED t
    GROUP BY t.CustomerId
)
SELECT
    c.CustomerId,
    c.CustomerExternalNo,
    c.FullName,
    c.PrestigeSegment,
    c.TotalCreditLimit,
    c.AvailableCreditLimit,
    CAST(c.AvailableCreditLimit / NULLIF(c.TotalCreditLimit, 0) AS DECIMAL(6,4)) AS AvailableLimitRatio,
    c.MisMonthlyNetProfit,
    c.HistoricalBaselineSpend,
    c.RealBaselineSpendIndex,
    c.SpendElasticityPersona,
    c.IsDormant,
    c.DaysSinceLastTx,
    c.HasMarketingConsent,
    c.PreferredRewardType,
    c.BehavioralClusterId,
    ISNULL(agg.ValidTxCount, 0) AS ValidTxCount,
    ISNULL(agg.TotalNominalSpend, 0.00) AS RecentNominalSpend,
    ISNULL(agg.DistinctSectorCount, 0) AS DistinctSectorCount,
    CAST(PERCENT_RANK() OVER (ORDER BY c.RealBaselineSpendIndex) AS DECIMAL(6,4)) AS PortfolioRealSpendQuantile,
    CAST(NTILE(10) OVER (ORDER BY c.RealBaselineSpendIndex) AS INT) AS SpendDecile
FROM dbo.CasualCampaign_CUSTOMER_PROFILE_MIS c
LEFT JOIN CustomerTxAgg agg ON c.CustomerId = agg.CustomerId;
GO

/* =========================================================================================
   4. INITIAL SEED DATA
   ========================================================================================= */

-- 4.1 Monthly Sectoral Inflation Index & Deflators
INSERT INTO dbo.CasualCampaign_INFLATION_INDEX_MONTHLY (YearMonth, SectorName, CpiIndexValue, DeflatorMultiplier)
VALUES
(202606, N'General',                  100.0000, 1.185000),
(202607, N'General',                  103.2000, 1.148256),
(202608, N'General',                  106.8000, 1.109551),
(202609, N'General',                  110.5000, 1.072398),
(202610, N'General',                  114.4000, 1.035839),
(202611, N'General',                  118.5000, 1.000000),
(202606, N'Grocery & Supermarket',    100.0000, 1.210000),
(202611, N'Grocery & Supermarket',    121.0000, 1.000000),
(202606, N'E-Commerce & Marketplace', 100.0000, 1.170000),
(202611, N'E-Commerce & Marketplace', 117.0000, 1.000000),
(202606, N'Fuel',                     100.0000, 1.160000),
(202611, N'Fuel',                     116.0000, 1.000000);

-- 4.2 Campaign Templates
INSERT INTO dbo.CasualCampaign_CAMPAIGN_TEMPLATE (
    CampaignCode, CampaignName, CampaignType, TargetSector, DefaultRewardType,
    TotalBudget, ConsumedBudget, MaxRewardPerCustomer, MinMisProfitThreshold,
    InterchangeMarginRate, StartDate, EndDate, IsActive, KillSwitchTriggered
)
VALUES
(N'CMP-2026-UPSELL-01', N'Personalized Total Spend Upsell (Causal Uplift) Campaign', N'TotalSpendUpsell',    N'All',                      N'RewardPoints', 2500000.00, 185000.00, 2500.00, -250.00, 0.0260, '2026-11-01', '2026-12-31', 1, 0),
(N'CMP-2026-SECTOR-02', N'E-Commerce and Grocery Sectoral Cross-Sell Campaign',      N'SectoralCrossSell',   N'E-Commerce & Marketplace', N'Cashback',     1200000.00,  92000.00, 1500.00,    0.00, 0.0280, '2026-11-01', '2026-12-31', 1, 0),
(N'CMP-2026-WAKEUP-03', N'Dormant Credit Card Reactivation Campaign',                N'DormantReactivation', N'All',                      N'Cashback',      800000.00,  45000.00, 1000.00, -500.00, 0.0250, '2026-11-01', '2026-12-31', 1, 0);

-- 4.3 3-Layer POS Enrichment Cache
INSERT INTO dbo.CasualCampaign_MERCHANT_MASTER_CACHE (
    RawMerchantKey, RawMcc_DE18, CleanedBrandName, ResolvedSector,
    SubMerchantProvider, ClassificationLayer, ConfidenceScore, IsCashLikeOrExcludedMcc
)
VALUES
(N'MIGROS TICARET A.S. ISTANBUL TR',       N'5411', N'MIGROS',        N'Grocery & Supermarket',    NULL,      1, 0.9990, 0),
(N'CARREFOURSA HIPERMARKET ATASEHIR',      N'5411', N'CARREFOURSA',   N'Grocery & Supermarket',    NULL,      1, 0.9980, 0),
(N'SHELL & TURCAS PETROL MASLAK',          N'5541', N'SHELL',         N'Fuel',                     NULL,      1, 0.9990, 0),
(N'OPET PETROLCULUK KADIKOY',              N'5541', N'OPET',          N'Fuel',                     NULL,      1, 0.9990, 0),
(N'IYZICO*TRENDYOL.COM ISTANBUL',          N'5399', N'TRENDYOL',      N'E-Commerce & Marketplace', N'IYZICO', 2, 0.9750, 0),
(N'PARAM*GETIR PERAKENDE LOJISTIK',        N'7399', N'GETIR',         N'Grocery & Supermarket',    N'PARAM',  2, 0.9680, 0),
(N'PAYTR*HEPSIBURADA D-MARKET',            N'5399', N'HEPSIBURADA',   N'E-Commerce & Marketplace', N'PAYTR',  2, 0.9790, 0),
(N'SIPAY*ZARA TURKIYE GIYIM',              N'5651', N'ZARA',          N'Apparel & Fashion',        N'SIPAY',  2, 0.9640, 0),
(N'MOKA*YEMEKSEPETI BANABI',               N'5812', N'YEMEKSEPETI',   N'Restaurant & Cafe',        N'MOKA',   2, 0.9610, 0),
(N'BEREKET UNLU MAMULLERI VE GIDA SAN',    N'5999', N'BEREKET GIDA',  N'Grocery & Supermarket',    NULL,      3, 0.9340, 0),
(N'ALTINBAS KUYUMCULUK KAPALICARSI',       N'6051', N'ALTINBAS',      N'Cash-Like / Excluded',     NULL,      1, 0.9950, 1),
(N'BILYONER INTERAKTIF HIZMETLER',         N'7995', N'BILYONER',      N'Cash-Like / Excluded',     NULL,      1, 0.9990, 1);

-- 4.4 Customer Portfolio Profiles
INSERT INTO dbo.CasualCampaign_CUSTOMER_PROFILE_MIS (
    CustomerExternalNo, FullName, CardNumberMasked, PrestigeSegment,
    TotalCreditLimit, AvailableCreditLimit, MisMonthlyNetProfit,
    HistoricalBaselineSpend, RealBaselineSpendIndex, SpendElasticityPersona,
    IsDormant, DaysSinceLastTx, HasMarketingConsent, PreferredRewardType, BehavioralClusterId
)
VALUES
(N'CUST-1001', N'Ahmet Yilmaz', N'5400 61** **** 1001', N'Gold',     150000.00, 110000.00, 4250.00, 24000.00, 25800.00, N'Persuadable',         0,   2, 1, N'RewardPoints', 1),
(N'CUST-1002', N'Zeynep Kaya',  N'5400 61** **** 1002', N'Silver',    85000.00,  62000.00, 2150.00, 14500.00, 15400.00, N'Persuadable',         0,   4, 1, N'Cashback',     2),
(N'CUST-1003', N'Mehmet Demir', N'5400 61** **** 1003', N'Private',  300000.00, 210000.00, 8900.00, 65000.00, 68200.00, N'FreeRider_SureThing', 0,   1, 1, N'RewardPoints', 3),
(N'CUST-1004', N'Elif Sahin',   N'5400 61** **** 1004', N'Gold',     120000.00, 120000.00, 5400.00,   450.00,   520.00, N'Dormant_Sleeper',     1, 115, 1, N'Cashback',     4),
(N'CUST-1005', N'Caner Ozturk', N'5400 61** **** 1005', N'Standard',  50000.00,  34000.00,  980.00,  9500.00, 10100.00, N'Persuadable',         0,   5, 1, N'RewardPoints', 2),
(N'CUST-1006', N'Selin Arslan', N'5400 61** **** 1006', N'Standard',  30000.00,   4500.00,  620.00,  8000.00,  8400.00, N'Persuadable',         0,   3, 1, N'Cashback',     1),
(N'CUST-1007', N'Burak Celik',  N'5400 61** **** 1007', N'Gold',     140000.00,  98000.00, 3900.00, 23500.00, 25100.00, N'Persuadable',         0,   2, 1, N'RewardPoints', 1),
(N'CUST-1008', N'Ayse Yildiz',  N'5400 61** **** 1008', N'Silver',    75000.00,  75000.00, 1850.00,     0.00,     0.00, N'Dormant_Sleeper',     1, 140, 1, N'RewardPoints', 4);

-- 4.5 Offer Assignments
INSERT INTO dbo.CasualCampaign_CUSTOMER_OFFER_ASSIGNMENT (
    CampaignId, CustomerId, MicroBucketId, MicroBucketName, ExperimentGroup,
    ExpectedOrganicSpend, PredictedUpliftSpend, PropensityScore, ReactivationProbability,
    AssignedTargetAmount, AssignedRewardAmount, RewardType, ExpectedNetIncrementalProfit,
    XaiReasonText, XaiTopFeaturesJson, AlternativeOffersJson, CustomerSelectedOfferIndex
)
VALUES
(1, 1001, 14, N'Bucket-14: Gold Segment High Uplift (36,000 TRY Target / 750 TRY Points)', N'Treatment',
 25800.00, 11400.00, 0.5800, 0.0000, 36000.00, 750.00, N'RewardPoints', 211.40,
 N'Based on your consistent Grocery and E-Commerce spending momentum (+18% real growth), Gold segment status, and 110,000 TRY available credit limit, an offer of 750 TRY Reward Points for a 36,000 TRY spend target was tailored for you.',
 N'[{"Feature":"RealBaselineSpendIndex","Impact":0.42},{"Feature":"PrestigeSegment_Gold","Impact":0.31},{"Feature":"EcommerceSpendTrend","Impact":0.27}]',
 N'[{"Index":1,"Title":"Spend 36,000 TRY to Earn 750 TRY Points","Target":36000,"Reward":750,"RewardType":"RewardPoints"},{"Index":2,"Title":"Spend 38,500 TRY to Earn 700 TRY Cashback","Target":38500,"Reward":700,"RewardType":"Cashback"},{"Index":3,"Title":"Spend 15,000 TRY in E-Commerce to Earn 800 TRY Points","Target":15000,"Reward":800,"RewardType":"RewardPoints"}]',
 1),
(2, 1002, 9, N'Bucket-09: Silver Segment Sectoral Cross-Sell (22,000 TRY Target / 450 TRY Cashback)', N'Treatment',
 15400.00, 7200.00, 0.6200, 0.0000, 22000.00, 450.00, N'Cashback', 151.60,
 N'Based on your E-Commerce transaction frequency across payment facilitators (Iyzico/PayTR) and your Cashback preference, 450 TRY Cashback was assigned for a 22,000 TRY spend target.',
 N'[{"Feature":"EcommerceTxFrequency","Impact":0.46},{"Feature":"PreferredReward_Cashback","Impact":0.34},{"Feature":"AvailableLimitRatio","Impact":0.20}]',
 N'[{"Index":1,"Title":"Spend 22,000 TRY to Earn 450 TRY Cashback","Target":22000,"Reward":450,"RewardType":"Cashback"},{"Index":2,"Title":"Spend 20,000 TRY to Earn 500 TRY Points","Target":20000,"Reward":500,"RewardType":"RewardPoints"},{"Index":3,"Title":"Spend 25,000 TRY to Earn 550 TRY Cashback","Target":25000,"Reward":550,"RewardType":"Cashback"}]',
 1),
(3, 1004, 2, N'Bucket-02: Dormant Card Reactivation (5,000 TRY First Spend / 200 TRY Cashback)', N'Treatment',
 520.00, 6800.00, 0.4500, 0.1450, 5000.00, 200.00, N'Cashback', 135.00,
 N'Because your card has been inactive for 115 days while maintaining high Gold segment MIS profitability, a reactivation offer of 200 TRY Cashback for a 5,000 TRY spend target was assigned.',
 N'[{"Feature":"DaysSinceLastTx_115","Impact":0.52},{"Feature":"MisMonthlyNetProfit","Impact":0.30},{"Feature":"ReactivationElasticity","Impact":0.18}]',
 N'[{"Index":1,"Title":"Spend 5,000 TRY to Earn 200 TRY Cashback","Target":5000,"Reward":200,"RewardType":"Cashback"},{"Index":2,"Title":"Spend 4,000 TRY to Earn 220 TRY Points","Target":4000,"Reward":220,"RewardType":"RewardPoints"},{"Index":3,"Title":"Spend 3,500 TRY in Grocery to Earn 210 TRY Cashback","Target":3500,"Reward":210,"RewardType":"Cashback"}]',
 1),
(1, 1007, 0, N'Bucket-00: Stratified Control Group (Hold-out RCT)', N'Control_Holdout',
 25100.00, 0.00, 0.5000, 0.0000, 0.00, 0.00, N'RewardPoints', 0.00,
 N'Assigned to the Stratified Randomized Control Group (Hold-out RCT) for CUPED net incremental economic value measurement.',
 N'[{"Feature":"StratifiedHoldoutGroup","Impact":1.00}]',
 N'[]',
 1);

-- 4.6 Progress Trackers
INSERT INTO dbo.CasualCampaign_OFFER_PROGRESS_TRACKER (
    AssignmentId, CustomerId, CampaignId, TargetAmount,
    CurrentQualifiedSpend, RemainingAmount, ProgressPercentage,
    QualifiedTxCount, ExcludedTxCount, IsCompleted, CompletedAt, LastTransactionAt
)
VALUES
(1, 1001, 1, 36000.00, 21500.00, 14500.00, 59.72, 4, 2, 0, NULL, DATEADD(HOUR, -3, SYSUTCDATETIME())),
(2, 1002, 2, 22000.00, 18000.00,  4000.00, 81.82, 3, 0, 0, NULL, DATEADD(HOUR, -5, SYSUTCDATETIME())),
(3, 1004, 3,  5000.00,  5200.00,     0.00,100.00, 2, 0, 1, DATEADD(HOUR, -1, SYSUTCDATETIME()), DATEADD(HOUR, -1, SYSUTCDATETIME()));

-- 4.7 Sample Transactions & Anti-Gaming Logs
INSERT INTO dbo.CasualCampaign_TRANSACTION_LEDGER_FEED (
    AuthorizationCode, CustomerId, TransactionTimestamp, Amount, Currency,
    RawMcc_DE18, RawMerchantName_DE43, ResolvedBrand, ResolvedSector,
    ClassificationLayer, IsReversalOrRefund, OriginalTransactionId,
    IsExcludedByAntiGaming, ExclusionReason, ProcessingLatencyMs
)
VALUES
(N'AUTH-9001', 1001, DATEADD(DAY, -5, SYSUTCDATETIME()), 6500.00, N'TRY', N'5411', N'MIGROS TICARET A.S. ISTANBUL TR', N'MIGROS',      N'Grocery & Supermarket',    1, 0, NULL, 0, NULL, 8.40),
(N'AUTH-9002', 1001, DATEADD(DAY, -4, SYSUTCDATETIME()), 8200.00, N'TRY', N'5399', N'IYZICO*TRENDYOL.COM ISTANBUL',    N'TRENDYOL',    N'E-Commerce & Marketplace', 2, 0, NULL, 0, NULL, 14.20),
(N'AUTH-9003', 1001, DATEADD(DAY, -2, SYSUTCDATETIME()), 3800.00, N'TRY', N'5541', N'SHELL & TURCAS PETROL MASLAK',    N'SHELL',       N'Fuel',                     1, 0, NULL, 0, NULL, 7.10),
(N'AUTH-9004', 1001, DATEADD(HOUR,-4, SYSUTCDATETIME()), 3000.00, N'TRY', N'7399', N'PARAM*GETIR PERAKENDE LOJISTIK',  N'GETIR',       N'Grocery & Supermarket',    2, 0, NULL, 0, NULL, 11.80),
(N'AUTH-9005', 1001, DATEADD(MINUTE,-225,SYSUTCDATETIME()),2500.00,N'TRY',N'7399', N'PARAM*GETIR PERAKENDE LOJISTIK',  N'GETIR',       N'Grocery & Supermarket',    2, 0, NULL, 1, N'Anti-Gaming: Split transaction detected at the same merchant within 30 minutes.', 9.60),
(N'AUTH-9006', 1001, DATEADD(DAY, -1, SYSUTCDATETIME()),12000.00, N'TRY', N'6051', N'ALTINBAS KUYUMCULUK KAPALICARSI', N'ALTINBAS',    N'Cash-Like / Excluded',     1, 0, NULL, 1, N'Anti-Gaming: Cash-like or excluded MCC code (6051).', 6.30),
(N'AUTH-9007', 1002, DATEADD(DAY, -3, SYSUTCDATETIME()),11000.00, N'TRY', N'5399', N'PAYTR*HEPSIBURADA D-MARKET',      N'HEPSIBURADA', N'E-Commerce & Marketplace', 2, 0, NULL, 0, NULL, 15.10),
(N'AUTH-9008', 1002, DATEADD(HOUR,-5, SYSUTCDATETIME()), 7000.00, N'TRY', N'5651', N'SIPAY*ZARA TURKIYE GIYIM',        N'ZARA',        N'Apparel & Fashion',        2, 0, NULL, 0, NULL, 13.40),
(N'AUTH-9009', 1004, DATEADD(HOUR,-1, SYSUTCDATETIME()), 5200.00, N'TRY', N'5399', N'IYZICO*TRENDYOL.COM ISTANBUL',    N'TRENDYOL',    N'E-Commerce & Marketplace', 2, 0, NULL, 0, NULL, 12.90);

-- 4.8 Reward Fulfillment Queue
INSERT INTO dbo.CasualCampaign_REWARD_FULFILLMENT_QUEUE (
    TrackerId, CustomerId, CampaignId, RewardAmount, RewardType, Status, ScheduledLoadDate
)
VALUES
(3, 1004, 3, 200.00, N'Cashback', N'PendingSettlement', DATEADD(DAY, 1, SYSUTCDATETIME()));
GO

PRINT N'CasualCampaign_ prefixed 8 tables, indexes, vw_CasualCampaign_CustomerCausalFeatures view, and initial seed data created successfully.';
GO
