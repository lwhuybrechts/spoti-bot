using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Bot.Upvotes
{
    public class Upvote : TableEntity
    {
        [IgnoreProperty]
        public string TrackId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        [IgnoreProperty]
        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
