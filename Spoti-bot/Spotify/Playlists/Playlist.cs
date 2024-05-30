using SpotiBot.Library;
using System.Runtime.Serialization;

namespace SpotiBot.Spotify.Playlists
{
    public class Playlist : MyTableEntity
    {
        [IgnoreDataMember]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Name { get; set; }
    }
}
