using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpotiBot.Api.Bot.Votes
{
    public interface IVoteService
    {
        bool IsAnyVoteCallback(UpdateDto updateDto);
        Task<BotResponseCode> TryHandleVote(UpdateDto updateDto);
        Task<(string, InlineKeyboardMarkup)> UpdateTextAndKeyboard(string text, InlineKeyboardMarkup keyboard, Track track);
    }
}