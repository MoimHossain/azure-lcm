
param location string = resourceGroup().location
param vnetName string
param defaultSubnetName string = 'default' // do NOT change this name
param webAppSubnetName string = 'appservice' 
param addressPrefix string = '10.0.0.0/16'
param defaultSubnetAddressPrefix string = '10.0.2.0/23'
param appServiceSubnetAddressPrefix string = '10.0.1.0/27'


var defaultNsgName = 'nsg-${vnetName}-${defaultSubnetName}'
var containerGroupNsgName = 'nsg-${vnetName}-containergroup'

module nsgDefault 'nsg.bicep' = {
  name: defaultNsgName
  params: {
    location: location
    nsgName: defaultNsgName
  }
}

module nsgContainerGroup 'nsg.bicep' = {
  name: containerGroupNsgName
  params: {
    location: location
    nsgName: containerGroupNsgName
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
      {
        name: webAppSubnetName
        properties: {
          addressPrefix: appServiceSubnetAddressPrefix
          networkSecurityGroup: {
            id: nsgContainerGroup.outputs.nsgId
          }
          delegations: [
            {
              name: 'Microsoft.Web/serverFarms'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
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

resource delegatedSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-02-01' existing = {
  parent: vnet
  name: webAppSubnetName
}


output vnetId string = vnet.id
output vnetName string = vnet.name
output defaultSubnetName string = defaultSubnet.name
output defaultSubnetId string = defaultSubnet.id
output delegatedSubnetName string = delegatedSubnet.name
output delegatedSubnetId string = delegatedSubnet.id

