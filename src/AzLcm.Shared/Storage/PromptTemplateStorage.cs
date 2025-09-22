



namespace AzLcm.Shared.Storage
{
    public class PromptTemplateStorage
    {
        public async Task<string> GetFeedPromptAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
            return
"""
You are Product Owner for the Life Cycle management team. 
You need to classify the following Azure Update feed:
    1. If any service is retired, unsupported or deprecated.
        a. When possible you should come up with azure policy (code snippet) to mitigate the impact.
    2. If any service became GA or Preview, you must make announcement.
    3. If you can't classify an update set updateKind to Unknown.
    4. Summarize the update info in markdown format 0 minimum 100 words. 
    5. Only respond with JSON content - never respond with free texts.

You MUST Produce response in JSON that follows the schema below:                
```
type {
    azureServiceNames: string[],
    updateKind: 'Retired' | 'Deprecated' | 'Unsupported' | 'GenerallyAvailable' | 'Preview' | 'Unknown',
    actionable: boolean,
    announcementRequired: boolean,
    actionableViaAzurePolicy: boolean,
    summaryMarkdown: string,
    mitigationInstructionMarkdown: string | null
}
```
Example response:
```
{
    "azureServiceNames": ["Log Analytics", "Azure Monitor"],
    "updateKind": "Retired",
    "actionable": true,
    "announcementRequired": true,
    "actionableViaAzurePolicy": true,
    "summaryMarkdown": "This update affects the following services: Log Analytics, Azure Monitor ... etc. etc.",
    "mitigationInstructionMarkdown": "You can mitigate further consuming the service with following policy: { ...policy code snippet } "
}
```
""";

        }
    }
}
