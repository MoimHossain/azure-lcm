param nsgName string 
param location string = resourceGroup().location

resource networkSecurityGroups 'Microsoft.Network/networkSecurityGroups@2023-02-01' = {
  name: nsgName
  location: location  
  properties: {
    securityRules: [
      // for now, no rules      
    ]
  }
}

output nsgName string = networkSecurityGroups.name
output nsgId string = networkSecurityGroups.id
