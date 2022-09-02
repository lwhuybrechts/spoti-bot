using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Tracks;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.Votes
{
    public interface IVoteService
    {
        bool IsAnyVoteCallback(UpdateDto updateDto);
        Task<BotResponseCode> TryHandleVote(UpdateDto updateDto);
        Task<(string, InlineKeyboardMarkup)> UpdateTextAndKeyboard(string text, InlineKeyboardMarkup keyboard, Track track);
    }
}