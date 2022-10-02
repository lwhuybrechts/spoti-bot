using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using SpotiBot.Spotify.Tracks;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpotiBot.Bot.Votes
{
    public interface IVoteService
    {
        bool IsAnyVoteCallback(UpdateDto updateDto);
        Task<BotResponseCode> TryHandleVote(UpdateDto updateDto);
        Task<(string, InlineKeyboardMarkup)> UpdateTextAndKeyboard(string text, InlineKeyboardMarkup keyboard, Track track);
    }
}