using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SpotiBot.Data.Models
{
    public class Vote : TableEntity
    {
        [IgnoreProperty]
        public string PlaylistId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public DateTimeOffset CreatedAt { get; set; }

        private int _type { get; set; }
        public int Type
        {
            get { return _type; }
            set
            {
                _type = value;
                SetRowKey();
            }
        }

        private string _trackId = string.Empty;
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

        // The RowKey is a concatenation of the TrackId, userId and Type, so set the RowKey in their setters.
        private void SetRowKey()
        {
            RowKey = $"{TrackId}_{UserId}_{Type}";
        }
    }
}
