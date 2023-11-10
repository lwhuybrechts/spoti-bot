using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.HandleUpdate
{
    public interface IHandleInlineQueryService
    {
        Task<BotResponseCode> TryHandleInlineQuery(UpdateDto updateDto);
    }
}