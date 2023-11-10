using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.HandleUpdate.Dto
{
    public interface IUpdateDtoService
    {
        Task<UpdateDto> Build(Telegram.Bot.Types.Update update);
    }
}