

using AzLcm.Shared.ServiceHealth;

namespace AzLcm.Shared.Storage
{
    public static class ServiceHealthEventExtensions
    {
        private static string GetPrimaryKey(this ServiceHealthEvent item)
        {
            var lastUpdate = item.LastUpdate.GetValueOrDefault();
            return $"{lastUpdate.Year}-{lastUpdate.Month}";
        }

        private static string GetRowKey(this ServiceHealthEvent item)
        {
            return item.Name;
        }

        public static (string partitionKey, string rowKey) GetKeyPair(this ServiceHealthEvent item)
        {
            return (GetPrimaryKey(item), GetRowKey(item));
        }
    }
}
