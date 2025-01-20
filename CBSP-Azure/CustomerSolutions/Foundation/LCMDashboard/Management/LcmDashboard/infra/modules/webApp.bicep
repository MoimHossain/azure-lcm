

param webAppName string
param location string = resourceGroup().location
param logAnalyticsWorkspaceId string
param uamiId string
param uamiClientId string
param delegatedSubnetId string
param keyVaultUri string
param storageAccountName string

var appServicePlanName = '${webAppName}svcPlan'
var appInsightName = '${webAppName}insight'


module serverPlan './webAppPlan.bicep' = {
  name: appServicePlanName
  params: {
    serverfarmName: appServicePlanName
    location: location
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightName
  location: location
  kind: 'string'
  tags: {
    displayName: 'AppInsight'
    ProjectName: webAppName
  }
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
}


resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uamiId}': {}
    }
  }
  properties: {
    enabled: true
    serverFarmId: serverPlan.outputs.serverfarmId
    reserved: false
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      acrUseManagedIdentityCreds: false
      alwaysOn: true
      http20Enabled: true
      functionAppScaleLimit: 0
      minimumElasticInstanceCount: 1  
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: true
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    ipMode: 'IPv4'
    vnetBackupRestoreEnabled: false
    containerSize: 0
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    endToEndEncryptionEnabled: false
    redundancyMode: 'None'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
    autoGeneratedDomainNameLabelScope: 'TenantReuse'
    virtualNetworkSubnetId: delegatedSubnetId
  }
}


// resource appServiceSiteExtension 'Microsoft.Web/sites/siteextensions@2020-06-01' = {
//   parent: webApp
//   name: 'Microsoft.ApplicationInsights.AzureWebSites'
//   dependsOn: [
//     appInsights
//   ]
// }



resource siteConfigs 'Microsoft.Web/sites/config@2022-09-01' = {
  name: 'appsettings'  
  kind: 'app'
  parent: webApp
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    USER_ASSIGNED_MANAGED_IDENTITY_CLIENT_ID: uamiClientId
    PROCESS_AZURE_FEED: 'true'
    PROCESS_AZURE_POLICY: 'true'
    PROCESS_AZURE_SERVICE_HEALTH: 'true'
    GITHUB_PAT: 'XXX'
    AZURE_KEY_VAULT_URI: keyVaultUri
    AZURE_POLICY_URI_BASE: 'https://api.github.com/repos/azure/azure-policy/contents/'
    AZURE_POLICY_PATH: 'built-in-policies/policyDefinitions'
    AZURE_UPDATE_FEED_URI: 'https://www.microsoft.com/releasecommunications/api/v2/azure/rss'
    AZURE_STORAGE_ACCOUNT_NAME: storageAccountName
    AZURE_STORAGE_FEED_TABLE_NAME: 'azupdatefeed'
    AZURE_STORAGE_POLICY_TABLE_NAME: 'azpolicy'
    AZURE_STORAGE_SVC_HEALTH_TABLE_NAME: 'azsvchealth'
    AZURE_STORAGE_COFIG_CONTAINER: 'az-lcm-configs'
    AZURE_OPENAI_GPT_DEPLOYMENT_ID: 'gpt-4o'
    AZURE_DEVOPS_ORGNAME: ''
    AZURE_DEVOPS_USE_PAT: 'true'
    AZURE_DEVOPS_PAT: ''
    AZURE_DEVOPS_USE_MANAGED_IDENTITY: 'false'
    AZURE_DEVOPS_CLIENT_ID_OF_MANAGED_IDENTITY: ''
    AZURE_DEVOPS_TENANT_ID_OF_MANAGED_IDENTITY: ''
    AZURE_DEVOPS_USE_SERVICE_PRINCIPAL: 'false'
    AZURE_DEVOPS_CLIENT_ID_OF_SERVICE_PRINCIPAL: ''
    AZURE_DEVOPS_CLIENT_SECRET_OF_SERVICE_PRINCIPAL: ''
    AZURE_DEVOPS_TENANT_ID_OF_SERVICE_PRINCIPAL: ''
    APPLICATION_INSIGHT_CONNECTION_STRING: appInsights.properties.ConnectionString
  } 
}



resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${webAppName}-diagnostics'
  scope: webApp
  properties: {
    logs: [
      {
        category: 'AppServiceAntivirusScanAuditLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceHTTPLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceConsoleLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceAppLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceFileAuditLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceAuditLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceIPSecAuditLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServicePlatformLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'AppServiceAuthenticationLogs'
        categoryGroup: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
    ]    
    metrics: [
      {
        timeGrain: null
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
        category: 'AllMetrics'
      }
    ]
    workspaceId: logAnalyticsWorkspaceId
  }
}

output webAppId string = webApp.id
output defaultHostName string = webApp.properties.defaultHostName

