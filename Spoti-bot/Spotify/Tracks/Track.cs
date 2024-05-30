using SpotiBot.Library;
using System;
using System.Runtime.Serialization;

namespace SpotiBot.Spotify.Tracks
{
    public class Track : MyTableEntity
    {
        [IgnoreDataMember]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }
        
        [IgnoreDataMember]
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

        [IgnoreDataMember]
        public TrackState State
        {
            get { return (TrackState)StateValue; }
            set { StateValue = (int)value; }
        }
    }
}