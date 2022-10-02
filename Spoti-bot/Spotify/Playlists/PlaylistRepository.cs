using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Playlists
{
    public class PlaylistRepository : BaseRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Playlist).Name), "playlists")
        {

        }
    }
}
