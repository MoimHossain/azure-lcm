using 'main.bicep'

var appname = readEnvironmentVariable('appname')
var appEnv = readEnvironmentVariable('appEnv')

param uamiName = '${appname}-uami-${appEnv}'
param containerRegistryName = '${appname}contregistry${appEnv}'
param keyvaultName = '${appname}keyvault${appEnv}'
param logAnalyticsName = '${appname}-log-analytics-${appEnv}'
param appInsightName = '${appname}-appinsights-${appEnv}'
param acaEnvName = '${appname}-appenv-${appEnv}'
param vnetName = '${appname}-vnet-${appEnv}'
param publicIpAddressName = '${appname}-publicip-${appEnv}'

param storageAccountName = '${appname}strgdemo001'
param storageContainerName = '${appname}'
param storageQueueName = '${appname}'
param storageSecKeyName = 'StorageKey'
param storageSecAccountName = 'StorageAccountName'
param storageSecContainerName = 'ContainerName'
