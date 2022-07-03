using Microsoft.WindowsAzure.Storage.Table;

namespace Spoti_bot.Spotify.Playlists
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
