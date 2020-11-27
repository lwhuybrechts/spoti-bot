using System.Threading.Tasks;

namespace Spoti_bot.Bot.Interfaces
{
    public interface IHandleCallbackQueryService
    {
        Task<BotResponseCode> TryHandleCallbackQuery(Telegram.Bot.Types.Update update);
    }
}
