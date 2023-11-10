using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class TrackRepository : BaseRepository<Track>, ITrackRepository
    {
        public TrackRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Track).Name), "")
        {
        }
    }
}
