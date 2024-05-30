using Azure;
using Azure.Data.Tables;
using System;

namespace SpotiBot.Library
{
    public class MyTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
