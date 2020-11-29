using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate
{
    public interface IHandleInlineQueryService
    {
        Task<BotResponseCode> TryHandleInlineQuery(Telegram.Bot.Types.Update update);
    }
}