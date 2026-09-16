getcustomerlimitinfo sadece fizik kartlar için ve kanal bilgisinde hangi limit varsa onu getiriyor.

--limit ağacı
SELECT * FROM [FORTUNA].[Limits].[LimitParameters] WITH(NOLOCK)

--genel limit ayarları
SELECT * FROM [FORTUNA].[Limits].[TransactionLimits] WITH(NOLOCK)
WHERE bitActive=1
    AND vchChannelName IN('TYFMOBAPP','TYFSHSMOB') 
    AND intGroupId = 1 -- 1- Bireysel - Standard, 16- Bireysel - Personel, 2-Tüzel
    AND intSecurityTypeId=1
    AND vchTransactionCode IN
    (
        'Transactions', 'EFT', 'IBANEFT', 'FastGatewaySendOrder','WireToIBAN',
        'QRCodeDepositTransactions', 'ATMQrDepositMessage', --'DepositWithBKMQRCode',
        'QRCodeTransactions','OnasQRCodeMessage','WithdrawalWithBKMQRCode'
    )
ORDER by vchTransactionCode, intSecurityTypeId

--Müşteri limit ayarları
SELECT * FROM [FORTUNA].[Limits].[CustomerLimits] WITH(NOLOCK) WHERE lngCustomerNo=4958790
SELECT * FROM [FORTUNA].[Limits].[CustomerInternetLimits] WITH(NOLOCK) WHERE lngCustomerNo=4958790
SELECT * FROM [FORTUNA].[Limits].[CustomerLimitsHistory] WITH(NOLOCK) WHERE lngCustomerNo=4958790

