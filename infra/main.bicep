targetScope = 'resourceGroup'

param location string = resourceGroup().location
param uamiName string 
param logAnalyticsName string
param storageAccountName string 
param containerRegistryName string
param openaiName string
param vnetName string
param keyvaultName string
param privateEndpointNameOpenAI string = 'privatelink-cognitiveservices'
param privateEndpointNameKeyVault string = 'privatelink-keyvault'


//var privateDnsZoneName = 'privatelink.openai.azure.com'
var keyVaultPrivateDNSZoneName = 'privatelink.vaultcore.azure.net'

//var networkInterfaceCardNameForOpenAI = '${privateEndpointNameOpenAI}-nic'
var networkInterfaceCardNameForKeyvault = '${keyVaultPrivateDNSZoneName}-nic'

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



// Private endpoint and DNS for KeyVault

@description('Private DNS Zone for custom entries')
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: commonPrivateDnsZoneName
  location: 'global'
}

@description('Private DNS Zone Virtual Network Link')
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: 'vnetlink'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnet.outputs.vnetId
    }
    registrationEnabled: false
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
    customNetworkInterfaceName: networkInterfaceCardNameForKeyvault
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
    customNetworkInterfaceName: networkInterfaceCardNameForOpenAI
  }
}










resource dnsZoneGroupKeyVault 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {
  parent: privateEndpointForKeyvault
  name: 'keyvault-dnsZoneGroup'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'keyvault-config'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}


resource keyVaultDnsRecord 'Microsoft.Network/privateDnsZones/A@2020-06-01' = {
  name: keyvaultName
  parent: privateDnsZone
  properties: {
    ttl: 300
    aRecords: [
      {
        ipv4Address: privateEndpointForKeyvault.properties.customDnsConfigs[0].ipAddresses[0]
      }
    ]
  }
}



resource dnsZoneGroupOpenAI 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {
  parent: privateEndpointForOpenAI
  name: 'openai-dnsZoneGroup'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'openai-config'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}


resource OpenAIDnsRecord 'Microsoft.Network/privateDnsZones/A@2020-06-01' = {
  name: openaiName
  parent: privateDnsZone
  properties: {
    ttl: 300
    aRecords: [
      {
        ipv4Address: privateEndpointForOpenAI.properties.customDnsConfigs[0].ipAddresses[0]
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


// module openai 'modules/openai.bicep' = {
//   name: openaiName
//   params: {
//     aoiName: openaiName
//     location: location
//     keyVaultName: keyvaultName
//   }
//   dependsOn: [
//     keyvault
//   ]
// }
