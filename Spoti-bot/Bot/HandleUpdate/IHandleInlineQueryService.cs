using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Bot.HandleUpdate
{
    public interface IHandleInlineQueryService
    {
        Task<BotResponseCode> TryHandleInlineQuery(UpdateDto updateDto);
    }
}