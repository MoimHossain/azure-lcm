
param vnetId string

var keyVaultPrivateDNSZoneName = 'privatelink.vaultcore.azure.net'

@description('Private DNS Zone for custom entries')
resource privateDnsZoneKeyVault 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: keyVaultPrivateDNSZoneName
  location: 'global'
}

@description('Private DNS Zone Virtual Network Link')
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZoneKeyVault
  name: 'vnetlinkPrivateDnsZoneKeyVault'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId 
    }
    registrationEnabled: true
  }
}

output dnsZoneId string = privateDnsZoneKeyVault.id
