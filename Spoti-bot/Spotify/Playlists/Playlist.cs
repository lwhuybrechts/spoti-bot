using Microsoft.WindowsAzure.Storage.Table;

namespace SpotiBot.Spotify.Playlists
{
    public class Playlist : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Name { get; set; }
    }
}
