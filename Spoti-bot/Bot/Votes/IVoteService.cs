using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Tracks;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Votes
{
    public interface IVoteService
    {
        bool IsAnyVoteCallback(UpdateDto updateDto);
        Task<BotResponseCode> TryHandleVote(UpdateDto updateDto);
        Task<string> UpdateWithVotes(string text, Track track);
    }
}