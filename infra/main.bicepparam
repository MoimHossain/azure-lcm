using 'main.bicep'

var workloadName = readEnvironmentVariable('workloadName')
var workloadEnv = readEnvironmentVariable('workloadEnv')

param uamiName = '${workloadName}-uami-${workloadEnv}'
param containerRegistryName = toLower('${workloadName}azcontregx${workloadEnv}')
param logAnalyticsName = toLower('${workloadName}-log-analytics-${workloadEnv}')
param storageAccountName = toLower('${workloadName}storageaccx${workloadEnv}') 
