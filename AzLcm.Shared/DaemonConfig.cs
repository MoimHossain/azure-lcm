

namespace AzLcm.Shared
{
    public class DaemonConfig
    {
        public string StorageConnectionString => ReadEnvironmentKey("AZURE_STORAGE_CONNECTION");
        public string StorageTableName => ReadEnvironmentKey("AZURE_STORAGE_TABLE_NAME");

        private string ReadEnvironmentKey(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Environment variable {key} is not set");
            }
            return value;
        }
    }
}


