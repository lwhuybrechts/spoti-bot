using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public interface IKeyboardService
    {
        InlineKeyboardMarkup CreateKeyboard();
        Task<InlineKeyboardMarkup> GetUpdatedKeyboard(Message message, string trackId);
    }
}