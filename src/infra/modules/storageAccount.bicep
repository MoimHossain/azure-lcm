param accountName string
param location string
param identityPrincipalId string

//storage account
resource mainstorage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: accountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []
    }    
    allowBlobPublicAccess: false
  }  
}




@description('This is the built-in Contributor role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles')
var blobDataContributorRoleDefinitionId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource blobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {  
  name: blobDataContributorRoleDefinitionId
}

resource blobDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: mainstorage
  name: guid('${accountName}-blobdatacontributor')
  properties: {
    roleDefinitionId: blobDataContributorRoleDefinition.id
    principalId: identityPrincipalId    
  }
}

@description('This is the built-in Contributor role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles')
var tableDataContributorRoleDefinitionId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource tableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {  
  name: tableDataContributorRoleDefinitionId
}

resource tableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: mainstorage
  name: guid('${accountName}-tabledatacontributor')
  properties: {
    roleDefinitionId: tableDataContributorRoleDefinition.id
    principalId: identityPrincipalId    
  }
}

@description('This is the built-in Contributor role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles')
var roleDefinitionId  = 'b24988ac-6180-42a0-ab88-20f7382dd24c'

resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {  
  name: roleDefinitionId
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: mainstorage
  name: guid('${accountName}-identity-contributor')
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: identityPrincipalId    
  }
}

resource defaultTableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: mainstorage
  name: 'default'
}

resource defaultBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: mainstorage
  name: 'default'
}

output accountName string = mainstorage.name
output storageAccountId string = mainstorage.id
