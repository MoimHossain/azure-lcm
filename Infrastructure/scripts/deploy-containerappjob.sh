#!/bin/bash


# export resourceGroupName=$resourceGroupName
# export location=$location
# export APP_NAME=$APP_NAME


echo "Starting script...$tag and image name $imageName"

echo "Starting deploying the Container app job provisioning..."


echo "Deploying app Bicep file..."
az deployment group create --resource-group $resourceGroupName --template-file 'Infrastructure/containerjob/app.bicep' --parameters 'Infrastructure/containerjob/app.bicepparam'