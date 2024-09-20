@description('Name of the Azure OpenAI resource')
param aoiName string
param location string = resourceGroup().location
param keyVaultName string

resource aoi 'Microsoft.CognitiveServices/accounts@2021-04-30' = {
  name: aoiName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: aoiName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }    
    publicNetworkAccess: 'Disabled'
  }
}

// resource model_gpt_4o 'Microsoft.CognitiveServices/accounts/deployments@2024-04-01-preview' = {
//   parent: aoi
//   name: 'gpt-4o'
//   sku: {
//     name: 'GlobalStandard'
//     capacity: 20
//   }  
//   properties: {    
//     model: {
//       format: 'OpenAI'
//       name: 'gpt-4o'
//       version: '2024-05-13'
//     }
//     versionUpgradeOption: 'OnceCurrentVersionExpired'
//     currentCapacity: 20
//   }
// }

module keySecretForAOIKey 'kvSecret.bicep' = {
  name: 'keySecretForAOIKey'
  params: {
    keyVaultName: keyVaultName
    secretName: 'AOIKey'
    secretValue: aoi.listKeys().key1
  }
}

module keySecretForEndpoint 'kvSecret.bicep' = {
  name: 'keySecretForEndpoint'
  params: {
    keyVaultName: keyVaultName
    secretName: 'AOIEndpoint'
    secretValue: aoi.properties.endpoint
  }
}


output aoiResourceId string = '${resourceGroup().id}/providers/Microsoft.CognitiveServices/accounts/${aoiName}'

