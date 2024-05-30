using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Playlists
{
    public class PlaylistRepository : BaseRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(Playlist).Name), "playlists")
        {

        }
    }
}
