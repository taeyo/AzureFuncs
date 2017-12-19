# AzureFuncs
repo for azure functions sample 

1. Function to get last modified blob name in blob container
  - need to add storage account key with name, "AzureStorage" to application setting
  - it does not work with Azure Functions 2.x(.net core 2.0)
    - it's because of Azure Storage SDK issue. but, possible to work in neat future.
  - please use Azure Function 1.x
  
