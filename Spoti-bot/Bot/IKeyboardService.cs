using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public interface IKeyboardService
    {
        InlineKeyboardMarkup CreateButtonKeyboard(string text, string url);
        InlineKeyboardMarkup CreatePostedTrackResponseKeyboard();
        Task<InlineKeyboardMarkup> GetUpdatedUpvoteKeyboard(Message message, string trackId);
    }
}