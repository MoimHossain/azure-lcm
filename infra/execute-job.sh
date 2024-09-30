#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv
# export keyvaultUri=$keyvaultUri
# export registryURI=$registryURI
# export containerName=$containerName
# export imageName=$imageName
# export imageTag=$imageTag
# export GTIHUB_PAT=$GTIHUB_PAT
# export STORAGE_ACCOUNT=$STORAGE_ACCOUNT
# export AZURE_DEVOPS_ORGNAME=$AZURE_DEVOPS_ORGNAME
# export AZURE_DEVOPS_PAT=$AZURE_DEVOPS_PAT

vnetName=${workloadName}vnet${workloadEnv}
workspaceName=${workloadName}-log-analytics-${workloadEnv}

echo "Starting Configuration Blob Uploading..."
echo "Resource Group: $resourceGroupName"
echo "Location: $location"
echo "Workload Name: $workloadName"
echo "Workload Environment: $workloadEnv"
echo "Container Name: $containerName"
echo "Key Vault URI: $keyvaultUri"
echo "Registry URI: $registryURI"
echo "Image Name: $imageName"
echo "Image Tag: $imageTag"
echo "Storage Account: $STORAGE_ACCOUNT"
echo "Azure DevOps Organization Name: $AZURE_DEVOPS_ORGNAME"
echo "Azure DevOps PAT: $AZURE_DEVOPS_PAT"
echo "GitHub PAT: $GTIHUB_PAT"
echo "VNET Name: $vnetName"

uamiId=$(az identity show --resource-group $resourceGroupName --name ${workloadName}-uami-${workloadEnv} | jq -r '.id')
vnetId=$(az network vnet show --resource-group $resourceGroupName --name $vnetName  --query id --output tsv)
subnetId=$(az network vnet subnet show --resource-group $resourceGroupName --vnet-name $vnetName --name containergroup --query id --output tsv)
workspaceId=$(az monitor log-analytics workspace show --resource-group $resourceGroupName --workspace-name $workspaceName --query customerId --output tsv)
workspaceKey=$(az monitor log-analytics workspace get-shared-keys --resource-group $resourceGroupName --workspace-name $workspaceName --query primarySharedKey --output tsv)



echo "UAMI ID: $uamiId"
echo "VNET ID: $vnetId"
echo "Subnet ID: $subnetId"
echo "Workspace ID: $workspaceId"
echo "Workspace Key: $workspaceKey"

az container create \
    --resource-group $resourceGroupName \
    --name azure-lcm-$imageTag \
    --image $registryURI/$imageName:$imageTag \
    --assign-identity $uamiId \
    --acr-identity $uamiId \
    --vnet $vnetId \
    --subnet $subnetId \
    --log-analytics-workspace $workspaceId \
    --log-analytics-workspace-key $workspaceKey \
    --ip-address Private \
    --restart-policy Never \
    --no-wait \
    --environment-variables \
    PROCESS_AZURE_SERVICE_HEALTH=false \
    PROCESS_AZURE_POLICY=false \
    PROCESS_AZURE_FEED=true \
    GITHUB_PAT="$GTIHUB_PAT" \
    AZURE_KEY_VAULT_URI="$keyvaultUri" \
    AZURE_POLICY_URI_BASE="https://api.github.com/repos/azure/azure-policy/contents/" \
    AZURE_POLICY_PATH="built-in-policies/policyDefinitions" \
    AZURE_UPDATE_FEED_URI="https://aztty.azurewebsites.net/rss/updates" \
    AZURE_STORAGE_ACCOUNT_NAME="$STORAGE_ACCOUNT" \
    AZURE_STORAGE_FEED_TABLE_NAME="azupdatefeed" \
    AZURE_STORAGE_POLICY_TABLE_NAME="azpolicy" \
    AZURE_STORAGE_SVC_HEALTH_TABLE_NAME="azsvchealth" \
    AZURE_STORAGE_COFIG_CONTAINER=$containerName \
    AZURE_OPENAI_GPT_DEPLOYMENT_ID=gpt-4o \
    AZURE_DEVOPS_ORGNAME=$AZURE_DEVOPS_ORGNAME \
    AZURE_DEVOPS_USE_PAT=true \
    AZURE_DEVOPS_USE_MANAGED_IDENTITY=false \
    AZURE_DEVOPS_USE_SERVICE_PRINCIPAL=false \
    AZURE_DEVOPS_PAT="$AZURE_DEVOPS_PAT"