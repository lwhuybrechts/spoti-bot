using System.Threading.Tasks;

namespace Spoti_bot.Bot
{
    public interface IHandleCallbackQueryService
    {
        Task<bool> TryHandleCallbackQuery(Telegram.Bot.Types.Update update);
    }
}
