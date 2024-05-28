targetScope = 'resourceGroup'

param location string = resourceGroup().location
param uamiName string 
param logAnalyticsName string
param storageAccountName string 
param containerRegistryName string

module uami 'modules/identity.bicep' = {
  name: uamiName
  params: {
    uamiName: uamiName
    location: location
  }
}


module containerRegistry  'modules/registry.bicep' = {
  name: containerRegistryName
  params: {
    location: location
    registryName: containerRegistryName
    skuName: 'Basic'
    userAssignedIdentityPrincipalId: uami.outputs.principalId
    adminUserEnabled: false
  }
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: logAnalyticsName
  params: {
    logAnalyticsName: logAnalyticsName
    localtion: location
  }
}

module storageAccount 'modules/storageAccount.bicep' = {
  name: storageAccountName
  params: {
    accountName: storageAccountName
    location: location
    identityPrincipalId: uami.outputs.principalId
  }
}
