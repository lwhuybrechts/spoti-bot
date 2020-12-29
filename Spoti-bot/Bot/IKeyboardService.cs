using Spoti_bot.Bot.Chats;
using Spoti_bot.Spotify.Tracks;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public interface IKeyboardService
    {
        InlineKeyboardMarkup CreateButtonKeyboard(string text, string url);
        InlineKeyboardMarkup CreateSwitchToPmKeyboard(Chat chat);
        InlineKeyboardMarkup CreatePostedTrackResponseKeyboard();
        Task<InlineKeyboardMarkup> GetUpdatedVoteKeyboard(InlineKeyboardMarkup inlineKeyboard, Track track);
    }
}