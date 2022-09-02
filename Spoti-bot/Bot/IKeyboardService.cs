using Spoti_bot.Bot.Chats;
using Spoti_bot.Spotify.Tracks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public interface IKeyboardService
    {
        InlineKeyboardMarkup CreateUrlKeyboard(string text, string url);
        InlineKeyboardMarkup CreateSwitchToPmKeyboard(Chat chat);
        InlineKeyboardMarkup CreatePostedTrackResponseKeyboard();
        InlineKeyboardMarkup CreateSeeVotesKeyboard(Track track);
        InlineKeyboardMarkup AddWebAppKeyboard();
        InlineKeyboardMarkup AddOrRemoveSeeVotesButton(InlineKeyboardMarkup inlineKeyboard, Track track, bool hasVotes);
        bool AreSame(InlineKeyboardMarkup firstKeyboard, InlineKeyboardMarkup secondKeyboard);
    }
}