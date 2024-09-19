
param location string = resourceGroup().location
param vnetName string
param defaultSubnetName string = 'default' // do NOT change this name
param addressPrefix string = '10.0.0.0/16'
param defaultSubnetAddressPrefix string = '10.0.2.0/23'


var defaultNsgName = 'nsg-${vnetName}-${defaultSubnetName}'

module nsgDefault 'nsg.bicep' = {
  name: defaultNsgName
  params: {
    location: location
    nsgName: defaultNsgName
  }
}

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2021-05-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        addressPrefix
      ]
    }
    subnets: [
      {
        name: defaultSubnetName
        properties: {
          addressPrefix: defaultSubnetAddressPrefix
          privateLinkServiceNetworkPolicies: 'Enabled'
          privateEndpointNetworkPolicies: 'Disabled'
          networkSecurityGroup: {
            id: nsgDefault.outputs.nsgId
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.CognitiveServices'
              locations: [
                location
              ]
            }
          ]
        }
      }
    ]
  }
}



resource vnet 'Microsoft.Network/virtualNetworks@2023-02-01' existing = {
  name: virtualNetwork.name
}

resource defaultSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-02-01' existing = {
  parent: vnet
  name: defaultSubnetName
}


output vnetId string = vnet.id
output vnetName string = vnet.name
output defaultSubnetName string = defaultSubnet.name
output defaultSubnetId string = defaultSubnet.id
