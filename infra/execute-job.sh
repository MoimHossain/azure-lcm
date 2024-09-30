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
# export AZURE_OPENAI_ENDPOINT=$AZURE_OPENAI_ENDPOINT
# export AZURE_OPENAI_API_KEY=$AZURE_OPENAI_API_KEY

uamiId=$(az identity show --resource-group $resourceGroupName --name ${workloadName}-uami-${workloadEnv} | jq -r '.id')
echo "UAMI ID: $uamiId"

CONNECTION_STRING=$(az storage account show-connection-string --resource-group $resourceGroupName --name $STORAGE_ACCOUNT --query connectionString --output tsv)

# Temporarily allowing access to storage account from all networks
az storage account update --resource-group $resourceGroupName --name $STORAGE_ACCOUNT --default-action Allow --public-network-access Enabled


# Check if the container exists
EXISTING_CONTAINER=$(az storage container list --connection-string $CONNECTION_STRING --account-name $STORAGE_ACCOUNT --query "[?name=='$containerName'].name" --output tsv)

# Create the container if it does not exist
if [ -z "$EXISTING_CONTAINER" ]; then
    echo "Container $containerName does not exist. Creating it now..."
    az storage container create --name $containerName --account-name $STORAGE_ACCOUNT
    echo "Container $containerName created."
else
    echo "Container $containerName already exists."
fi



LOCAL_DIRECTORY="./infra/configurations"
# Upload files from the directory to the container with overwrite option
for FILE in "$LOCAL_DIRECTORY"/*; do
    if [ -f "$FILE" ]; then
        FILE_NAME=$(basename "$FILE")
        echo "Uploading $FILE_NAME to $containerName..."
        az storage blob upload --connection-string $CONNECTION_STRING  --account-name $STORAGE_ACCOUNT --container-name $containerName --file "$FILE" --name "$FILE_NAME" --overwrite
    fi
done

echo "All files uploaded successfully."
az storage account update --resource-group $resourceGroupName --name $STORAGE_ACCOUNT --default-action Deny --public-network-access Disabled


# az container create \
#     --resource-group $resourceGroupName \
#     --name azure-lcm-$imageTag \
#     --image $registryURI/$imageName:$imageTag \
#     --assign-identity $uamiId \
#     --acr-identity $uamiId \
#     --ip-address Private \
#     --restart-policy Never \
#     --no-wait \
#     --environment-variables \
#     PROCESS_AZURE_SERVICE_HEALTH=true \
#     PROCESS_AZURE_POLICY=false \
#     PROCESS_AZURE_FEED=false \
#     GITHUB_PAT="$GTIHUB_PAT" \
#     AZURE_KEY_VAULT_URI="$keyvaultUri" \
#     AZURE_POLICY_URI_BASE="https://api.github.com/repos/azure/azure-policy/contents/" \
#     AZURE_POLICY_PATH="built-in-policies/policyDefinitions" \
#     AZURE_UPDATE_FEED_URI="https://azurecomcdn.azureedge.net/en-us/updates/feed/" \
#     AZURE_STORAGE_ACCOUNT_NAME="$STORAGE_ACCOUNT" \
#     AZURE_STORAGE_FEED_TABLE_NAME="azupdatefeed" \
#     AZURE_STORAGE_POLICY_TABLE_NAME="azpolicy" \
#     AZURE_STORAGE_SVC_HEALTH_TABLE_NAME="azsvchealth" \
#     AZURE_STORAGE_COFIG_CONTAINER=$containerName \
#     AZURE_OPENAI_GPT_DEPLOYMENT_ID=gpt-4o \
#     AZURE_DEVOPS_ORGNAME=$AZURE_DEVOPS_ORGNAME \
#     AZURE_DEVOPS_USE_PAT=true \
#     AZURE_DEVOPS_USE_MANAGED_IDENTITY=false \
#     AZURE_DEVOPS_USE_SERVICE_PRINCIPAL=false \
#     AZURE_DEVOPS_PAT="$AZURE_DEVOPS_PAT" 
