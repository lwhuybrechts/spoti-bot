using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Bot.Votes
{
    public interface IVoteRepository
    {
        Task<Vote> Get(Vote item);
        Task<List<Vote>> GetVotes(string playlistId, string trackId);
        Task<List<Vote>> GetAllByPartitionKey(string partitionKey);
        Task Upsert(Vote item);
        Task Delete(Vote item);
        Task Delete(List<Vote> items);
    }
}