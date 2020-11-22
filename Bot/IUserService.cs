using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot
{
    public interface IUserService
    {
        Task<Data.User.User> SaveUser(Message message);
    }
}