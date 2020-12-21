using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public interface IUserService
    {
        Task<User> Get(long id);
        Task<User> SaveUser(User user, long chatId);
        Task<List<User>> GetUpvoteUsers(string trackId);
    }
}