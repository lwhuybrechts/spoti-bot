﻿using SpotiBot.Library;
using System;
using System.Runtime.Serialization;

namespace SpotiBot.Bot.Votes
{
    public class Vote : MyTableEntity
    {
        [IgnoreDataMember]
        public string PlaylistId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Used int since Azure Table Storage doesn't support enums.
        /// </summary>
        public int TypeValue { get; set; }

        [IgnoreDataMember]
        public VoteType Type {
            get { return (VoteType)TypeValue; }
            set
            {
                TypeValue = (int)value;
                SetRowKey();
            }
        }

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

        // The RowKey is a concatenation of the TrackId, userId and Type, so set the RowKey in their setters.
        private void SetRowKey()
        {
            RowKey = $"{TrackId}_{UserId}_{TypeValue}";
        }
    }
}
