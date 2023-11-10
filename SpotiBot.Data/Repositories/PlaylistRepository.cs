using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class PlaylistRepository : BaseRepository<Playlist>, IPlaylistRepository
    {
        public PlaylistRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Playlist).Name), "playlists")
        {
        }
    }
}
