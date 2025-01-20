using 'main.bicep'

var workloadName = readEnvironmentVariable('workloadName')
var workloadEnv = readEnvironmentVariable('workloadEnv')

param uamiName = '${workloadName}-uami-${workloadEnv}'
param logAnalyticsName = toLower('${workloadName}-log-analytics-${workloadEnv}')
param storageAccountName = toLower('${workloadName}storageacc${workloadEnv}') 
param openaiName = toLower('${workloadName}openai${workloadEnv}')
param vnetName = toLower('${workloadName}vnet${workloadEnv}')
param keyvaultName = toLower('${workloadName}keyvault${workloadEnv}')
param webAppName = toLower('${workloadName}webapp${workloadEnv}')
