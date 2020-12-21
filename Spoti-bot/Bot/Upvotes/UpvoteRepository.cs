using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Upvotes
{
    public class UpvoteRepository : BaseRepository<Upvote>, IUpvoteRepository
    {
        public UpvoteRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Upvote).Name), null)
        {
            
        }

        public Task<List<Upvote>> GetUpvotes(string playlistId, string trackId)
        {
            var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(Upvote.PartitionKey), QueryComparisons.Equal, playlistId);
            var trackIdFilter = TableQuery.GenerateFilterCondition(nameof(Upvote.TrackId), QueryComparisons.Equal, trackId);

            var query = new TableQuery<Upvote>().Where(partitionKeyFilter).Where(trackIdFilter);

            return ExecuteSegmentedQueries(query);
        }
    }
}
