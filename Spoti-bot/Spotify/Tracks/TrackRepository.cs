using Microsoft.WindowsAzure.Storage.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Spotify.Tracks
{
    public class TrackRepository : BaseRepository<Track>, ITrackRepository
    {
        public TrackRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Track).Name), null)
        {
        }
    }
}
