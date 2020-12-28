using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Votes
{
    public class VoteRepository : BaseRepository<Vote>, IVoteRepository
    {
        public VoteRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Vote).Name), null)
        {

        }

        public Task<List<Vote>> GetVotes(string playlistId, string trackId)
        {
            var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(Vote.PartitionKey), QueryComparisons.Equal, playlistId);
            var trackIdFilter = TableQuery.GenerateFilterCondition(nameof(Vote.TrackId), QueryComparisons.Equal, trackId);

            var filter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, trackIdFilter);
            var query = new TableQuery<Vote>().Where(filter);

            return ExecuteSegmentedQueries(query);
        }
    }
}
