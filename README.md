# Azure-Lcm
Lifecycle management for Azure services

## Overview

Lifecycle management for Azure services is a set of scripts that can be used to manage the lifecycle of Azure services. The scripts are written in Python and can be used to create, update, delete, and list Azure services. The scripts are designed to be used in a CI/CD pipeline to automate the deployment of Azure services.

## Prerequisites

- Azure CLI
- Azure DevOps (pipelines)

## Usage

The scripts can be used to create, update, delete, and list Azure services. The scripts are designed to be used in a CI/CD pipeline to automate the deployment of Azure services. You start with [build-infra.sh](./infra/build-infra.sh) script.


The environment variables that need to be set are:

```bash
export resourceGroupName=$resourceGroupName
export location=$location
export workloadName=$workloadName
export workloadEnv=$workloadEnv
```

The scripts can be run using the following commands:

```bash
./infra/build-infra.sh
```

This will produce the following Azure Resources:

- Resource Group
- User Assigned Managed Identity
- Log Analytics Workspace
- Virtual Network
    - Subnet - Delegated to Azure Container Group
    - Subnet - To host Private Endpoints
- Storage Account
    - Role Assignments
    - Blob Storage
        - Private DNS Zone
        - Private Endpoint
        - Private Link
    - Table Storage
        - Private DNS Zone
        - Private Endpoint
        - Private Link
- Key Vault
    - Role Assignments
    - Private DNS Zone
    - Private Endpoint
    - Private Link
- Azure Open AI
    - Model GPT 4o will be deployed
    - Private DNS Zone
    - Private Endpoint
    - Private Link
    - Keep Secret in Key Vault (endpoint and key)
- Container Registry (Not using Private Container Registry - so Azure Pipeline can reach to it)


## Building Docker image

The script [build-container.sh](./infra/build-container.sh) will build the Docker image and push it to the Azure Container Registry.

## Running container instance

The script [execute-job.sh](./infra/execute-job.sh) will run the container instance in Azure Container Instance.

> Note: The script [execute-job.sh](./infra/execute-job.sh) As it is now, will try to create containers into the Blob storage, which will not work because the storage account is only accepting traffic from vnet. Hence, you need to create the container as one-time process while keep the storage account's - allow traffic from anywhere - and then change it back to vnet only.

