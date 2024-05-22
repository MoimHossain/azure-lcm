targetScope = 'resourceGroup'
// Parameters
param containerappsjobname string

param tags object = {
  IaC: 'Bicep'
  Demo: 'Azure Container Apps Jobs'
}
param imageName string
param tagName string
param containerRegistryName string 
param location string = resourceGroup().location
param acaEnvName string 
param uamiName string
param appInsightName string

param processorParallelism int = 5
param replicaRetryLimit int = 1
param replicaTimeout int = 300

param useAzureDevopsManagedIdenity bool = false
param useAzureDevopsUseServicePrincipal bool = false
param azureDevopsUsePat bool = false

// Existing Resources
resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = { name: containerRegistryName }
resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = { name: uamiName }
resource acaEnvironment 'Microsoft.App/managedEnvironments@2022-11-01-preview'  existing = {   name: acaEnvName }
resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = { name: appInsightName }

// Variables
var containerRegistry = '${containerRegistryName}.azurecr.io'
var containerImage = '${containerRegistryName}.azurecr.io/${imageName}:${tagName}'

// Modules

module processorJob 'container-apps-job.bicep' = {
  name: 'processorJob'
  params: {
    containerappsjobname: containerappsjobname
    location: location       
    userAssignedIdentityName: uami.name
    containerImage: containerImage
    containerRegistry: containerRegistry
    triggerType: 'Schedule'
    cronExpression: '*/5 * * * *'
    parallelism: processorParallelism
    replicaCompletionCount: processorParallelism
    replicaRetryLimit: replicaRetryLimit
    replicaTimeout: replicaTimeout
    environmentName: acaEnvironment.name    
    tags: tags
    hasIdentity: true
    isPrivateRegistry: true
    containerRegistryUsername: ''
    registryPassword: ''    
    useManagedIdentityForImagePull: true
    registries: [
      {
        server: acr.properties.loginServer
        identity: uami.id
      }
    ]
    env: [
      {
        name: 'AZURE_CLIENT_ID'
        value: uami.properties.clientId
      }   
      {
        name: 'APPINSIGHT_CONN_STR'
        value: appInsights.properties.ConnectionString
      }
      {
        name: 'AZURE_DEVOPS_ORGNAME'
        value: 'damayantibhuyan'
      } 
      {
        name: 'AZURE_DEVOPS_USE_MANAGED_IDENTITY'
        value: useAzureDevopsManagedIdenity
      } 
      {
        name: 'AZURE_DEVOPS_CLIENT_ID_OF_MANAGED_IDENTITY'
        value: uami.properties.clientId
      }   
      {
        name: 'AZURE_DEVOPS_TENANT_ID_OF_MANAGED_IDENTITY'
        value: uami.properties.tenantId
      }  
      {
        name: 'AZURE_DEVOPS_USE_SERVICE_PRINCIPAL'
        value: useAzureDevopsUseServicePrincipal
      }
      {
        name: 'AZURE_DEVOPS_CLIENT_ID_OF_SERVICE_PRINCIPAL'
        value: 'spnclientid'
      }
      {
        name: 'AZURE_DEVOPS_CLIENT_SECRET_OF_SERVICE_PRINCIPAL'
        value: 'spnsecret'
      }
      {
        name: 'AZURE_DEVOPS_TENANT_ID_OF_SERVICE_PRINCIPAL'
        value: 'spntenantid'
      }
      {
        name: 'AZURE_DEVOPS_TENANT_ID_OF_SERVICE_PRINCIPAL'
        value: 'spntenantid'
      }
      {
        name: 'AZURE_DEVOPS_USE_PAT'
        value: 'azureDevopsUsePat'
      }
      {
        name: 'AZURE_DEVOPS_PAT'
        value: 'azureDevopsPat'
      }
      
    ]
  }
}


