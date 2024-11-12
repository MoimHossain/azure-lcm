#!/bin/bash

# export resourceGroupName=$resourceGroupName

containers=$(az container list --resource-group $resourceGroupName --query "[].{name:name}" -o tsv)

if [ -n "$containers" ]; then
  # Iterate over each container and delete it
  for container in $containers
  do
    echo "Deleting container instance: $container"
    az container delete --name $container --resource-group $resourceGroupName --yes
  done
  echo "All container instances in resource group $resourceGroupName have been deleted."
else
  echo "No container instances found in resource group $resourceGroupName."
fi