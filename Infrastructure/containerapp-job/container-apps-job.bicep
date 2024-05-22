// Parameters

param containerappsjobname string
param location string = resourceGroup().location
param tags object
param workloadProfileName string = ''

param hasIdentity bool
param isPrivateRegistry bool
param containerRegistry string
param userAssignedIdentityName string
param environmentName string
param containerRegistryUsername string
param registryPassword string
param useManagedIdentityForImagePull bool = false

param volumes array = []
param registries array = []
param replicaRetryLimit	int = 1
param replicaTimeout	int = 60
param secrets	array = []

@allowed([
  'Event'
  'Manual'
  'Schedule'
])
param triggerType string = 'Schedule'
param cronExpression	string = '*/5 * * * *'
param parallelism	int = 1
param replicaCompletionCount	int = 1

param args array = []
param command array = []
param env array = []
param containerImage	string
param containerName	string = 'main'

param cpu string = '0.25'
param memory string = '0.5Gi'
param volumeMounts array = []

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: userAssignedIdentityName
}
resource environment 'Microsoft.App/managedEnvironments@2022-11-01-preview' existing = {
  name: environmentName
}

// Resources
resource job 'Microsoft.App/jobs@2023-04-01-preview' = {
  name: toLower(containerappsjobname)
  location: location
  tags: tags
  identity: hasIdentity ? {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uami.id}': {}
    }
  } : null
  properties: {
    configuration: {    
      scheduleTriggerConfig: triggerType == 'Schedule' ? {
        cronExpression: cronExpression
        replicaCompletionCount: replicaCompletionCount
        parallelism: parallelism
      } : null      
    
      registries: isPrivateRegistry ? [
        {
          server: containerRegistry
          identity: useManagedIdentityForImagePull ? uami.id : null
          username: useManagedIdentityForImagePull ? null : containerRegistryUsername
          passwordSecretRef: useManagedIdentityForImagePull ? null : registryPassword
        }   
      ] : null
      replicaRetryLimit: replicaRetryLimit
      replicaTimeout: replicaTimeout
      secrets: secrets
      triggerType: triggerType
    }
    environmentId: environment.id
    template: {
      containers: [
        {
          args: args
          command: command
          env: env
          image: containerImage
          name: containerName
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          volumeMounts: volumeMounts
        }
      ]
      volumes: volumes
    }
    workloadProfileName: workloadProfileName
  }
}

// Outputs
output id string = job.id
output name string = job.name
