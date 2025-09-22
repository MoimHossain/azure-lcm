

using AzLcm.Shared.Logging;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AzLcm.Shared.Storage
{
    public abstract class StorageBase
    {
        private TableClient? _tableClient = null;
        protected abstract ILogger Logger { get; }

        protected TableClient TableClient
        {
            get
            {
                if (_tableClient == null)
                {
                    try
                    {
                        _tableClient = GetTableClientCore();
                        Logger.LogStorageOperation("TableClientInitialized", GetStorageTableName());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogCriticalError(ex, "Failed to initialize table client", 
                            new { StorageAccount = GetStorageAccountName(), TableName = GetStorageTableName() });
                        throw;
                    }
                }
                return _tableClient;
            }
        }

        protected abstract string GetStorageAccountName();
        protected abstract string GetStorageTableName();

        protected abstract AzureCredentialProvider GetAzureCredentialProvider();

        protected virtual TableClient GetTableClientCore()
        {
            try
            {                
                return new(new Uri($"https://{GetStorageAccountName()}.table.core.windows.net"),
                    GetStorageTableName(),
                    GetAzureCredentialProvider().GetStorageCredential());
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError(ex, "Failed to create table client", 
                    new { StorageAccount = GetStorageAccountName(), TableName = GetStorageTableName() });
                throw;
            }
        }

        public async Task EnsureTableExistsAsync(CancellationToken cancellationToken)
        {
            using var scope = Logger.BeginOperationScope("EnsureTableExists", new { TableName = GetStorageTableName() });
            
            try
            {
                Logger.LogOperationStart("EnsureTableExists", new { TableName = GetStorageTableName() });
                await TableClient.CreateIfNotExistsAsync(cancellationToken);
                Logger.LogOperationSuccess("EnsureTableExists", TimeSpan.Zero, new { TableName = GetStorageTableName() });
            }
            catch (Exception ex)
            {
                Logger.LogOperationFailure("EnsureTableExists", ex, new { TableName = GetStorageTableName() });
                throw;
            }
        }
    }
}
