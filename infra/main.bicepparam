using 'main.bicep'

var workloadName = readEnvironmentVariable('workloadName')
var workloadEnv = readEnvironmentVariable('workloadEnv')

param uamiName = '${workloadName}-uami-${workloadEnv}'
param containerRegistryName = toLower('${workloadName}azcontreg${workloadEnv}')
param logAnalyticsName = toLower('${workloadName}-log-analytics-${workloadEnv}')
param storageAccountName = toLower('${workloadName}storageacc${workloadEnv}') 
