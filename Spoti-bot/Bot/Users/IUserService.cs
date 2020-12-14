using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public interface IUserService
    {
        Task<User> Get(long id);
        Task<User> SaveUser(Telegram.Bot.Types.User telegramUser);
        Task<List<User>> GetUpvoteUsers(string trackId);
    }
}