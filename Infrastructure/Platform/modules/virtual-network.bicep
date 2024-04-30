
param location string = resourceGroup().location
param vnetName string
param defaultSubnetName string = 'default' // do NOT change this name
param apimSubnetName string = 'apimsubnet'
param addressPrefix string = '10.0.0.0/16'
param containerSubnetAddressPrefix string = '10.0.2.0/23'
param apimAddressPrefix string = '10.0.1.0/27'

var apimNsgName = 'nsg-${vnetName}-${apimSubnetName}'
var defaultNsgName = 'nsg-${vnetName}-${defaultSubnetName}'

module nsgApim 'NetworkSecurityGroups/apim-subnet-nsg.bicep' = {
  name: apimNsgName
  params: {
    location: location
    nsgName: apimNsgName
  }
}

module nsgDefault 'NetworkSecurityGroups/apim-subnet-nsg.bicep' = {
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
          addressPrefix: containerSubnetAddressPrefix
          networkSecurityGroup: {
            id: nsgDefault.outputs.nsgId
          }
        }
      }
      {
        name: apimSubnetName
        properties: {
          addressPrefix: apimAddressPrefix
          networkSecurityGroup: {
            id: nsgApim.outputs.nsgId
          }
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

resource apimSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-02-01' existing = {
  parent: vnet
  name: apimSubnetName
}

output vnetId string = vnet.id

output defaultSubnetName string = defaultSubnet.name
output defaultSubnetId string = defaultSubnet.id

output apimSubnetName string = apimSubnet.name
output apimSubnetId string = apimSubnet.id
