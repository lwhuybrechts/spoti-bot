using Microsoft.WindowsAzure.Storage.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Spotify.Playlists
{
    public class PlaylistRepository : BaseRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Playlist).Name), "playlists")
        {

        }
    }
}
