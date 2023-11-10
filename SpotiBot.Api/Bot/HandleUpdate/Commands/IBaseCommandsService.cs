using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.HandleUpdate.Commands
{
    public interface IBaseCommandsService
    {
        bool IsAnyCommand(string text);
        Task<BotResponseCode> TryHandleCommand(UpdateDto updateDto);
    }
}
