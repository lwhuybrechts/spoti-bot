using Spoti_bot.Library;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.Upvotes
{
    public interface IUpvoteService
    {
        bool IsUpvoteCallback(CallbackQuery callbackQuery);
        Task<BotResponseCode> TryHandleUpvote(CallbackQuery callbackQuery);
    }
}