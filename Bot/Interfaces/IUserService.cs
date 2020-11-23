using Spoti_bot.Bot.Data.Users;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Interfaces
{
    public interface IUserService
    {
        Task<User> SaveUser(Telegram.Bot.Types.User telegramUser);
    }
}