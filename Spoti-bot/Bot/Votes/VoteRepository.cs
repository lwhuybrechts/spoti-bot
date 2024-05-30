using Azure.Data.Tables;
using SpotiBot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Bot.Votes
{
    public class VoteRepository : BaseRepository<Vote>, IVoteRepository
    {
        public VoteRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(Vote).Name), null)
        {

        }

        public Task<List<Vote>> GetVotes(string playlistId, string trackId)
        {
            return QueryPageable(x => x.PartitionKey == playlistId && x.TrackId == trackId);
        }
    }
}
