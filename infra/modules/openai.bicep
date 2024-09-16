@description('Name of the Azure OpenAI resource')
param aoiName string
param location string = resourceGroup().location


resource aoi 'Microsoft.CognitiveServices/accounts@2021-04-30' = {
  name: aoiName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }    
    publicNetworkAccess: 'Enabled'
  }
}

resource model_gpt_4o 'Microsoft.CognitiveServices/accounts/deployments@2024-04-01-preview' = {
  parent: aoi
  name: 'gpt-4o'
  sku: {
    name: 'GlobalStandard'
    capacity: 20
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-05-13'
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
    currentCapacity: 20
  }
}



