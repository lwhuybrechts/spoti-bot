using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.Interfaces
{
    public interface IUserService
    {
        Task<Data.User.User> SaveUser(Message message);
    }
}