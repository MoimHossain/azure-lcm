
param location string = resourceGroup().location
param vnetId string
param subnetId string
param storageAccountId string
param storageAccountName string

var net = 'net'
var windows = 'windows'
var core = 'core'

var tablePrivateDNSZoneName = 'privatelink.table.${core}.${windows}.${net}'
var blobPrivateDNSZoneName = 'privatelink.blob.${core}.${windows}.${net}'
var privateEndpointNameTableStorage = 'privatelink-table-storage'
var privateEndpointNameBlobStorage = 'privatelink-blob-storage'

@description('Private DNS Zone for storagge table')
resource privateDnsZoneTableStorage 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: tablePrivateDNSZoneName
  location: 'global'
}

@description('Private DNS Zone Virtual Network Link')
resource privateDnsZoneLinkTableStorage 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZoneTableStorage
  name: 'vnetlinkPrivateDnsZoneTableStorage'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId 
    }
    registrationEnabled: false
  }
}

resource privateEndpointForTableStorage 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  location: location
  name: privateEndpointNameTableStorage
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointNameTableStorage
        properties: {
          privateLinkServiceId: storageAccountId
          groupIds: [
            'table'
          ]
        }
      }
    ]
    customNetworkInterfaceName: '${storageAccountName}-table-nic'
  }
}

resource privateDnsZoneGroupsTableStorage 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {  
  parent: privateEndpointForTableStorage
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: privateEndpointNameTableStorage
        properties: {
          privateDnsZoneId: privateDnsZoneTableStorage.id
        }
      }
    ]
  }
}




@description('Private DNS Zone for storagge blob')
resource privateDnsZoneBlobStorage 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: blobPrivateDNSZoneName
  location: 'global'
}

@description('Private DNS Zone Virtual Network Link')
resource privateDnsZoneLinkBlobStorage 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZoneBlobStorage
  name: 'vnetlinkPrivateDnsZoneBlobStorage'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId 
    }
    registrationEnabled: false
  }
}


resource privateEndpointForBlobStorage 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  location: location
  name: privateEndpointNameBlobStorage
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointNameBlobStorage
        properties: {
          privateLinkServiceId: storageAccountId
          groupIds: [
            'blob'
          ]
        }
      }
    ]
    customNetworkInterfaceName: '${storageAccountName}-blob-nic'
  }
}

resource privateDnsZoneGroupsBlobStorage 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-03-01' = {  
  parent: privateEndpointForBlobStorage
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: privateEndpointNameBlobStorage
        properties: {
          privateDnsZoneId: privateDnsZoneBlobStorage.id
        }
      }
    ]
  }
}



output tableStorageDnsZoneId string = privateDnsZoneTableStorage.id
output blobStorageDnsZoneId string = privateDnsZoneBlobStorage.id
