using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Bot.HandleUpdate
{
    public interface IHandleCallbackQueryService
    {
        Task<BotResponseCode> TryHandleCallbackQuery(UpdateDto updateDto);
    }
}
