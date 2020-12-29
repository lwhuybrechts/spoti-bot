using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public interface IUserService
    {
        Task<User> SaveUser(User user, long chatId);
    }
}