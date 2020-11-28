using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Spotify.Data.Tracks
{
    public class Track : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Name { get; set; }
        public string FirstArtistName { get; set; }
        public string AlbumName { get; set; }
        public int AddedByTelegramUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}