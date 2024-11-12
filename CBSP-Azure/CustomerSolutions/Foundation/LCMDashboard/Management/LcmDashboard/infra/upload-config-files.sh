#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv
# export containerName=$containerName
# export STORAGE_ACCOUNT=$STORAGE_ACCOUNT

echo "Starting Configuration Blob Uploading..."
echo "Resource Group: $resourceGroupName"
echo "Location: $location"
echo "Workload Name: $workloadName"
echo "Workload Environment: $workloadEnv"
echo "Storage Account: $STORAGE_ACCOUNT"
echo "Container Name: $containerName"


CONNECTION_STRING=$(az storage account show-connection-string --resource-group $resourceGroupName --name $STORAGE_ACCOUNT --query connectionString --output tsv)
# Check if the container exists
EXISTING_CONTAINER=$(az storage container list --connection-string $CONNECTION_STRING --account-name $STORAGE_ACCOUNT --query "[?name=='$containerName'].name" --output tsv)

echo $EXISTING_CONTAINER

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
