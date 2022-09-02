using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Votes
{
    public interface IVoteRepository
    {
        Task<Vote> Get(Vote item);
        Task<List<Vote>> GetVotes(string playlistId, string trackId);
        Task<List<Vote>> GetAllByPartitionKey(string partitionKey);
        Task<Vote> Upsert(Vote item);
        Task Delete(Vote item);
        Task Delete(List<Vote> items);
    }
}