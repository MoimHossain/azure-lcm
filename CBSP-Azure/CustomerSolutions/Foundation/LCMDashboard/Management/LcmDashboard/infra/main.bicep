targetScope = 'resourceGroup'

param location string = resourceGroup().location
param uamiName string 
param logAnalyticsName string
param storageAccountName string 
param openaiName string
param vnetName string
param keyvaultName string
param webAppName string

var privateEndpointNameOpenAI = 'privatelink-cognitiveservices'
var privateEndpointNameKeyVault = 'privatelink-keyvault'


module uami 'modules/identity.bicep' = {
  name: uamiName
  params: {
    uamiName: uamiName
    location: location
  }
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: logAnalyticsName
  params: {
    logAnalyticsName: logAnalyticsName
    localtion: location
  }
}

module vnet 'modules/vnet.bicep'= {
  name: vnetName
  params: {
    vnetName: vnetName
    location: location
  }
}


module webapp 'modules/webApp.bicep' = {
  name: webAppName
  params: {
    webAppName: webAppName
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.laWorkspaceId
    uamiId: uami.outputs.resourceId
    delegatedSubnetId: vnet.outputs.delegatedSubnetId
  }
}



module privateDnsZoneKeyVault 'modules/privateDnsKeyVault.bicep' = {
  name: 'privateDnsKeyVault'
  params: {    
    vnetId: vnet.outputs.vnetId
  }
}

module privateDnsZoneOpenAI 'modules/privateDnsOpenAI.bicep' = {
  name: 'privateDnsOpenAI'
  params: {    
    vnetId: vnet.outputs.vnetId
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

resource privateEndpointForKeyvault 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  location: location
  name: privateEndpointNameKeyVault
  properties: {
    subnet: {
      id: vnet.outputs.defaultSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointNameKeyVault
        properties: {
          privateLinkServiceId: keyvault.outputs.keyVaultId
          groupIds: [
            'vault'
          ]
        }
      }
    ]
    customNetworkInterfaceName: '${keyvaultName}-nic'
  }
}

resource privateDnsZoneGroupsKeyVault 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {  
  parent: privateEndpointForKeyvault
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: privateEndpointNameKeyVault
        properties: {
          privateDnsZoneId: privateDnsZoneKeyVault.outputs.dnsZoneId
        }
      }
    ]
  }
}



module openai 'modules/openai.bicep' = {
  name: openaiName
  params: {
    aoiName: openaiName
    location: location
    keyVaultName: keyvault.outputs.keyVaultName
  }
}

resource privateEndpointForOpenAI 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  location: location
  name: privateEndpointNameOpenAI
  properties: {
    subnet: {
      id: vnet.outputs.defaultSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointNameOpenAI
        properties: {
          privateLinkServiceId: openai.outputs.aoiResourceId
          groupIds: [
            'account'
          ]
        }
      }
    ]
    customNetworkInterfaceName: '${openaiName}-nic'
  }
}

resource privateDnsZoneGroupsOpenAI 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {  
  parent: privateEndpointForOpenAI
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: privateEndpointNameOpenAI
        properties: {
          privateDnsZoneId: privateDnsZoneOpenAI.outputs.dnsZoneId
        }
      }
    ]
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


module privateEndpointStorage 'modules/privateEndpointStorage.bicep' = {
  name: 'privateEndpointStorage'
  params: {
    location: location
    storageAccountId: storageAccount.outputs.storageAccountId
    storageAccountName: storageAccount.outputs.accountName
    vnetId: vnet.outputs.vnetId
    subnetId: vnet.outputs.defaultSubnetId
  }
}
