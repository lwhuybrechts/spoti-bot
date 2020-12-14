using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Upvotes
{
    public interface IUpvoteRepository : IBaseRepository<Upvote>
    {
        Task<List<Upvote>> GetUpvotes(long userId);
    }
}