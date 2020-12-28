using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Votes
{
    public interface IVoteRepository : IBaseRepository<Vote>
    {
        Task<List<Vote>> GetVotes(string playlistId, string trackId);
    }
}