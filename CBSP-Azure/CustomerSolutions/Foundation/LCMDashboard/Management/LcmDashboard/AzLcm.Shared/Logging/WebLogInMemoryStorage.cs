

namespace AzLcm.Shared.Logging
{
    public class WebLogInMemoryStorage
    {
        private readonly Queue<WebLogEntry> webLogEntries = new();
        public void AddLogEntry(WebLogEntry webLogEntry)
        {
            webLogEntries.Enqueue(webLogEntry);
            if (webLogEntries.Count > 100)
            {
                webLogEntries.Dequeue();
            }
        }
        public IEnumerable<WebLogEntry> GetLogEntries()
        {
            return webLogEntries;
        }
    }
}