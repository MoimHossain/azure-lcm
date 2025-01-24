
param serverfarmName string 
param location string = resourceGroup().location
param logAnalyticsWorkspaceId string
@description('The SKU configuration for the App Service Plan.')
param skuConfig object = {
  name: 'P0v3'
  tier: 'Premium0V3'
  size: 'P0v3'
  family: 'Pv3'
  capacity: 1
}

resource serverfarm 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: serverfarmName
  location: location
  sku: skuConfig
  kind: 'app'
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
}


resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'serverfarm-diagnostics'
  scope: serverfarm
  properties: {
    metrics: [
      {
        timeGrain: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
        category: 'AllMetrics'
      }
    ]
    workspaceId: logAnalyticsWorkspaceId
  }
}


output serverfarmId string = serverfarm.id
