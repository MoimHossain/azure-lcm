#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv

echo "Starting Infrastructure provisioning..."
echo $resourceGroupName

echo "Creating resource group..."
az group create --name $resourceGroupName --location $location

echo "Deploying main Bicep file..."
#az deployment group create --confirm-with-what-if --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam

az deployment group create --resource-group $resourceGroupName --template-file main.bicep  --parameters main.bicepparam