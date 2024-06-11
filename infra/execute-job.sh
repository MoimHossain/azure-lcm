#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv
# export registryURI=$registryURI
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


az container create \
    --resource-group $resourceGroupName \
    --name azure-lcm-$imageTag \
    --image $registryURI/$imageName:$imageTag \
    --assign-identity $uamiId \
    --acr-identity $uamiId \
    --ip-address Private \
    --restart-policy Never \
    --no-wait \
    --environment-variables \
    PROCESS_AZURE_SERVICE_HEALTH=true \
    PROCESS_AZURE_POLICY=false \
    PROCESS_AZURE_FEED=false \
    GITHUB_PAT="$GTIHUB_PAT" \
    AZURE_POLICY_URI_BASE="https://api.github.com/repos/azure/azure-policy/contents/" \
    AZURE_POLICY_PATH="built-in-policies/policyDefinitions" \
    AZURE_UPDATE_FEED_URI="https://azurecomcdn.azureedge.net/en-us/updates/feed/" \
    AZURE_STORAGE_CONNECTION="$CONNECTION_STRING" \
    AZURE_STORAGE_FEED_TABLE_NAME="azupdatefeed" \
    AZURE_STORAGE_POLICY_TABLE_NAME="azpolicy" \
    AZURE_STORAGE_SVC_HEALTH_TABLE_NAME="azsvchealth" \
    AZURE_OPENAI_ENDPOINT="$AZURE_OPENAI_ENDPOINT" \
    AZURE_OPENAI_API_KEY="$AZURE_OPENAI_API_KEY" \
    AZURE_OPENAI_GPT_DEPLOYMENT_ID=gpt-35-turbo \
    AZURE_OPENAI_DAVINCI_DEPLOYMENT_ID=text-davinci-003 \
    AZURE_DEVOPS_ORGNAME=$AZURE_DEVOPS_ORGNAME \
    AZURE_DEVOPS_USE_PAT=true \
    AZURE_DEVOPS_USE_MANAGED_IDENTITY=false \
    AZURE_DEVOPS_USE_SERVICE_PRINCIPAL=false \
    AZURE_DEVOPS_PAT="$AZURE_DEVOPS_PAT" \
    SERVICE_HEALTH_KUSTO_QUERY="https://gist.githubusercontent.com/MoimHossain/b4e0af1ddf8230ae8fe7629fbbd1c562/raw/60bb16d1707f1113ebfe3fc9ab23a8b8449f1e42/serviceHealth-Kusto.txt" \
    SERVICE_HEALTH_CONFIG_URI="https://gist.githubusercontent.com/MoimHossain/b4e0af1ddf8230ae8fe7629fbbd1c562/raw/60bb16d1707f1113ebfe3fc9ab23a8b8449f1e42/azure-resource-graph-config.json" \
    SERVICE_HEALTH_WORKITEM_TEMPLATE_URI="https://gist.githubusercontent.com/MoimHossain/929bff56a45f31466eb1cd7bc05d9248/raw/e33bcc924549c5b1ba707e815e924f8d41baf377/service-health-workitem-template.json" \
    FEED_PROMPT_TEMPLATE_URI="https://gist.githubusercontent.com/MoimHossain/929bff56a45f31466eb1cd7bc05d9248/raw/8031be143fbe58ce63bb6d5ad61c8146bb299366/FeedPromptTemplate.txt" \
    FEED_WORKITEM_TEMPLATE_URI="https://gist.githubusercontent.com/MoimHossain/929bff56a45f31466eb1cd7bc05d9248/raw/44b483059962ccbac9e36250c0ff1bd66543f592/FeedCustomTemplate.json" \
    POLICY_WORKITEM_TEMPLATE_URI="https://gist.githubusercontent.com/MoimHossain/929bff56a45f31466eb1cd7bc05d9248/raw/89cc625548129d8da69bb47a908a9baac56a5f76/PolicyCustomTemplate.json"
