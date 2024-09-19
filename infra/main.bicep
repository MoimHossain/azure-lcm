targetScope = 'resourceGroup'

param location string = resourceGroup().location
param uamiName string 
param logAnalyticsName string
param storageAccountName string 
param containerRegistryName string
param openaiName string
param vnetName string
param keyvaultName string
param privateEndpointName string = 'privatelink-cognitiveservices'

var privateDnsZoneName = 'privatelink.openai.azure.com'
var networkInterfaceCardName = '${privateEndpointName}-nic1'

module uami 'modules/identity.bicep' = {
  name: uamiName
  params: {
    uamiName: uamiName
    location: location
  }
}

module keyvault 'modules/keyvault.bicep' = {
  name: keyvaultName
  params: {
    keyVaultName: keyvaultName
    objectId: uami.outputs.principalId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    keysPermissions: [
      'get'
      'list'
    ]
    secretsPermissions: [
      'get'
      'list'
    ]
    location: location
    skuName: 'standard'  
  }
}

module vnet 'modules/vnet.bicep'= {
  name: vnetName
  params: {
    vnetName: vnetName
    location: location
  }
}

module openai 'modules/openai.bicep' = {
  name: openaiName
  params: {
    aoiName: openaiName
    location: location
    keyVaultName: keyvaultName
  }
  dependsOn: [
    keyvault
  ]
}



resource privateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  location: location
  name: privateEndpointName
  properties: {
    subnet: {
      id: vnet.outputs.defaultSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointName
        properties: {
          privateLinkServiceId: openai.outputs.aoiResourceId
          groupIds: [
            'account'
          ]
        }
      }
    ]
    customNetworkInterfaceName: networkInterfaceCardName
  }
}


resource privateDnsZone 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZoneName
  location: 'global'  
  properties: {}
}

resource privateDnsZoneVnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {  
  location: 'global'
  parent: privateDnsZone
  name: vnetName
  properties: {
    registrationEnabled: true
    virtualNetwork: {
      id: vnet.outputs.vnetId
    }
  }
}

resource privateDnsZoneGroups 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {  
  parent: privateEndpoint
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: privateEndpointName
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}






// module containerRegistry  'modules/registry.bicep' = {
//   name: containerRegistryName
//   params: {
//     location: location
//     registryName: containerRegistryName
//     skuName: 'Basic'
//     userAssignedIdentityPrincipalId: uami.outputs.principalId
//     adminUserEnabled: false
//   }
// }

// module logAnalytics 'modules/log-analytics.bicep' = {
//   name: logAnalyticsName
//   params: {
//     logAnalyticsName: logAnalyticsName
//     localtion: location
//   }
// }

// module storageAccount 'modules/storageAccount.bicep' = {
//   name: storageAccountName
//   params: {
//     accountName: storageAccountName
//     location: location
//     identityPrincipalId: uami.outputs.principalId
//   }
// }
