using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Upvotes
{
    public interface IHandleCallbackQueryService
    {
        Task<BotResponseCode> TryHandleCallbackQuery(Telegram.Bot.Types.Update update);
    }
}
