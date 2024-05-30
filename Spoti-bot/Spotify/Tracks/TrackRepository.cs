using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Tracks
{
    public class TrackRepository : BaseRepository<Track>, ITrackRepository
    {
        public TrackRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(Track).Name), null)
        {
        }
    }
}
