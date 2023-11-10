using SpotiBot.Library.BusinessModels.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.Api.Bot
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