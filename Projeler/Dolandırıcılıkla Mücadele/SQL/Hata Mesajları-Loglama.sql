SELECT TOP (1000) [CreatedOn],*
  FROM [DB_Odeabank_DVP_Monitor].[dbo].[SystemLog]
  where MethodName like 'Odeabank.DVP.Cards.Domain.Services.FraudPreventionService%'
  order by Id desc
  --where --RequestUrl = 'http://authority-feature-2937.appdev.ocpdev.odeabank.com.tr/api/seal/verify' and
  --CorrelationId = 'ce82b46c-3714-497c-a656-1d3eaa1d3d49'
  
  --333815055
  --9592889