using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate
{
    public interface IHandleMessageService
    {
        Task<BotResponseCode> TryHandleMessage(UpdateDto updateDto);
    }
}
