use [DB_Odeabank_DVP_Authority]

SELECT TOP (1000) [Id]
      ,[State]
      ,[CreatedOn]
      ,[CreatedBy]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[CustomerNo]
      ,[FeatureKeyId]
      ,[Description]
  FROM [DB_Odeabank_DVP_Authority].[dbo].[CommonPilotWhiteList]
  where CustomerNo = 6161812
  --where CustomerNo = 3026667
  --3026667
   

