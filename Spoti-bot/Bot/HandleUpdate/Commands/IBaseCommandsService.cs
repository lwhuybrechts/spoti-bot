using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public interface IBaseCommandsService
    {
        bool IsAnyCommand(string text);
        Task<BotResponseCode> TryHandleCommand(UpdateDto updateDto);
    }
}
