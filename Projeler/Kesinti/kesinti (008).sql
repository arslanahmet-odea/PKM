--EFT Son 10 dakika hata oranı
use DB_Odeabank_DVP_Monitor
SELECT  
COALESCE(100*(CAST(SUM(ERROR_FLAG) AS decimal) / CAST(SUM(TOTAL_FLAG) as decimal)),0) AS ERROR_PERCENTAGE
FROM
(
	SELECT 
		1 AS TOTAL_FLAG,
		CASE CHARINDEX('errorMessage', ResponseBody) WHEN 0 THEN 1 ELSE 0 END SUCCESS_FLAG,
		CASE CHARINDEX('errorMessage', ResponseBody) WHEN 0 THEN 0 ELSE 1 END ERROR_FLAG
	FROM dbo.SystemLog TT WITH (NOLOCK)
	WHERE CreatedOn > DATEADD(minute, -40, GETDATE())
	AND  CreatedOn < GETDATE()
	AND RequestUrl in('http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/virtual-cards/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/security/settings/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/eft/creditcard/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/payment/other/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/payment/other/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/payment/own/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/waiting-provision-and-term-transaction-infos'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/detail'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/statement'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/installment/transactions?creditCardNo=5223********5926'  
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/statement/detail'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/cash-advance/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/installments-postpones/transactions'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/virtual-cards'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/virtual-cards/cancel/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/virtual-cards/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/security/pin/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/application/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/application/additinional/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/cash-advance/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/settings/accounting-period/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/settings/auto-payment/confirm'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards?'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/detail'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/waiting-provision-and-term-transaction-infos'
				,'http://dvp-ms-banking-cards.cloud.ocp.odeabank.com.tr/api/cards/creditcards/application/cash-advance/cancel/confirm'
	)
) AS TT

--EFT Son 10 dakika hata alan kayıtlar
SELECT * 
FROM dbo.SystemLog TT WITH (NOLOCK)
WHERE CreatedOn > DATEADD(minute, -10, GETDATE())
AND  CreatedOn < GETDATE()
AND RequestUrl in('http://dvp-ms-banking-transfers.cloud.ocp.odeabank.com.tr/api/eft/iban/confirm',
'http://dvp-ms-banking-transfers.cloud.ocp.odeabank.com.tr/api/eft/account/confirm',
'http://dvp-ms-banking-transfers.cloud.ocp.odeabank.com.tr/api/eft/name/confirm')
AND CHARINDEX('errorMessage', ResponseBody) > 1

