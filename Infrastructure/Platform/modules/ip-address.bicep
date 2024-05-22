param name string
param location string = resourceGroup().location
param sku string = 'Standard'
param publicIpAllocationMethod string = 'Static'
param domainNameLabel string

resource publicIpAddress 'Microsoft.Network/publicIPAddresses@2018-10-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  properties: {    
    publicIPAllocationMethod: publicIpAllocationMethod
    dnsSettings: {
      domainNameLabel: domainNameLabel
    }
  }
}

output publicIpAddress string = publicIpAddress.properties.ipAddress
output publicIpAddressId string = publicIpAddress.id

