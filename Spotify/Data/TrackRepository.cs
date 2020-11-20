using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Spotify.Data
{
    public class TrackRepository : BaseRepository<Track>, ITrackRepository
    {
        public TrackRepository(CloudTableClient cloudTableClient)
            // TODO: use playlistId as the partitionKey.
            : base(cloudTableClient.GetTableReference(typeof(Track).Name), "tracks")
        {
        }
    }
}
