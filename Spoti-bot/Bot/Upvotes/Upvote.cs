using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Bot.Upvotes
{
    public class Upvote : TableEntity
    {
        [IgnoreProperty]
        public string PlaylistId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public DateTimeOffset CreatedAt { get; set; }

        private string _trackId;
        public string TrackId
        {
            get { return _trackId; }
            set
            {
                _trackId = value;
                SetRowKey();
            }
        }

        private long _userId;
        public long UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                SetRowKey();
            }
        }

        // The RowKey is a concatenation of the TrackId and UserId, so set the RowKey in their setters.
        private void SetRowKey()
        {
            RowKey = $"{TrackId}_{UserId}";
        }
    }
}
