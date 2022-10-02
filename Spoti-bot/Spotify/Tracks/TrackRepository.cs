using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Tracks
{
    public class TrackRepository : BaseRepository<Track>, ITrackRepository
    {
        public TrackRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Track).Name), null)
        {
        }
    }
}
