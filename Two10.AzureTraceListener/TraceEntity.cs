using Microsoft.WindowsAzure.Storage.Table;

namespace Two10.AzureTraceListener
{
    public class TraceEntity : TableEntity
    {
        public string Value { get; set; }
        public string Category { get; set; }
    }
}
