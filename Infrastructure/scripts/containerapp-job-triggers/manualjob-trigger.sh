az containerapp job create \
    --name "my-job" \
    --resource-group "my-resource-group"  \
    --environment "my-environment" \
    --trigger-type "Manual" \
    --replica-timeout 1800 \
    --replica-retry-limit 1 \
    --replica-completion-count 1 \
    --parallelism 1 \
    --image "mcr.microsoft.com/k8se/quickstart-jobs:latest" \
    --cpu "0.25" --memory "0.5Gi"