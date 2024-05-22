param nsgName string 
param location string = resourceGroup().location

resource networkSecurityGroups 'Microsoft.Network/networkSecurityGroups@2023-02-01' = {
  name: nsgName
  location: location  
  properties: {
    securityRules: [
      {
        name: 'AllowTagHTTPSInbound'
        id: resourceId('Microsoft.Network/networkSecurityGroups/securityRules', nsgName, 'AllowTagHTTPSInbound')
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          description: 'Client communication to API Management gateway'
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '443'
          sourceAddressPrefix: 'Internet'
          destinationAddressPrefix: 'VirtualNetwork'
          access: 'Allow'
          priority: 4088
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'AllowTagCustom3443Inbound'
        id: resourceId('Microsoft.Network/networkSecurityGroups/securityRules', nsgName, 'AllowTagCustom3443Inbound')
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          description: 'Management endpoint for Azure portal and PowerShell'
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '3443'
          sourceAddressPrefix: 'ApiManagement'
          destinationAddressPrefix: 'VirtualNetwork'
          access: 'Allow'
          priority: 4087
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'AllowTagCustom6390Inbound'
        id: resourceId('Microsoft.Network/networkSecurityGroups/securityRules', nsgName, 'AllowTagCustom6390Inbound')
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
        properties: {
          description: 'Azure Infrastructure Load Balancer'
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '6390'
          sourceAddressPrefix: 'AzureLoadBalancer'
          destinationAddressPrefix: 'VirtualNetwork'
          access: 'Allow'
          priority: 4086
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
    ]
  }
}

output nsgName string = networkSecurityGroups.name
output nsgId string = networkSecurityGroups.id
