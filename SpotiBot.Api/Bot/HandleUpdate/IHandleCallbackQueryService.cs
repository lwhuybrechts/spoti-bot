using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.HandleUpdate
{
    public interface IHandleCallbackQueryService
    {
        Task<BotResponseCode> TryHandleCallbackQuery(UpdateDto updateDto);
    }
}
