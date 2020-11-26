using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.Interfaces
{
    public interface IUpvoteService
    {
        bool IsUpvoteCallback(CallbackQuery callbackQuery);
        Task<bool> TryHandleUpvote(CallbackQuery callbackQuery);
    }
}