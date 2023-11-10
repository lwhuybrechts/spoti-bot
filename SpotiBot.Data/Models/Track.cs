using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SpotiBot.Data.Models
{
    public class Track : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        [IgnoreProperty]
        public string PlaylistId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Name { get; set; } = string.Empty;
        public string FirstArtistName { get; set; } = string.Empty;
        public string AlbumName { get; set; } = string.Empty;
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int State { get; set; }
    }
}