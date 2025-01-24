

using Azure.Data.Tables;
using Azure.Identity;

namespace AzLcm.Shared.Storage
{
    public abstract class StorageBase
    {


        private TableClient? _tableClient = null;

        protected TableClient TableClient
        {
            get
            {
                _tableClient ??= GetTableClientCore();
                return _tableClient;
            }
        }

        protected abstract string GetStorageAccountName();
        protected abstract string GetStorageTableName();

        protected abstract AzureCredentialProvider GetAzureCredentialProvider();

        protected virtual TableClient GetTableClientCore()
        {
            return new(new Uri($"https://{GetStorageAccountName()}.table.core.windows.net"),
                GetStorageTableName(),
                GetAzureCredentialProvider().GetStorageCredential());
        }

        public async Task EnsureTableExistsAsync(CancellationToken cancellationToken)
        {
            await TableClient.CreateIfNotExistsAsync(cancellationToken);
        }
    }
}
