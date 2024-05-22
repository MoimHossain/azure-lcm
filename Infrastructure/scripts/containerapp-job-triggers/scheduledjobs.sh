# Expression	Description
# 0 */2 * * *	Runs every two hours.
# 0 0 * * *	    Runs every day at midnight.
# 0 0 * * 0	    Runs every Sunday at midnight.
# 0 0 1 * *	    Runs on the first day of every month at midnight.



az containerapp job create \
    --name "my-job" \
    --resource-group "my-resource-group"  \
    --environment "my-environment" \
    --trigger-type "Schedule" \
    --replica-timeout 1800 \
    --replica-retry-limit 1 \
    --replica-completion-count 1 \
    --parallelism 1 \
    --image "mcr.microsoft.com/k8se/quickstart-jobs:latest" \
    --cpu "0.25" --memory "0.5Gi" \
    --cron-expression "0 0 * * *"