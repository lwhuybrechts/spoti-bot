using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Bot.Data.Upvotes
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
        public int UserId
        {
            get { return int.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
