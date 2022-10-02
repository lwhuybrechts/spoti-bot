using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SpotiBot.Spotify.Tracks
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

        public string Name { get; set; }
        public string FirstArtistName { get; set; }
        public string AlbumName { get; set; }
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        
        /// <summary>
        /// Used int since Azure Table Storage doesn't support enums.
        /// </summary>
        public int StateValue { get; set; }

        [IgnoreProperty]
        public TrackState State
        {
            get { return (TrackState)StateValue; }
            set { StateValue = (int)value; }
        }
    }
}