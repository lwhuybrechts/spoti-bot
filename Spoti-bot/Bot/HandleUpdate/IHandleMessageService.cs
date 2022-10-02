using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Bot.HandleUpdate
{
    public interface IHandleMessageService
    {
        Task<BotResponseCode> TryHandleMessage(UpdateDto updateDto);
    }
}
