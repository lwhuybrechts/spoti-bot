using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Bot.HandleUpdate.Commands
{
    public interface IBaseCommandsService
    {
        bool IsAnyCommand(string text);
        Task<BotResponseCode> TryHandleCommand(UpdateDto updateDto);
    }
}
