
param vnetId string

var openAIPrivateDNSZoneName = 'privatelink.openai.azure.com'


@description('Private DNS Zone for custom entries')
resource privateDnsZoneOpenAI 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: openAIPrivateDNSZoneName
  location: 'global'
}

@description('Private DNS Zone Virtual Network Link')
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZoneOpenAI
  name: 'vnetlinkPrivateDnsZoneOpenAI'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId 
    }
    registrationEnabled: false
  }
}

output dnsZoneId string = privateDnsZoneOpenAI.id
