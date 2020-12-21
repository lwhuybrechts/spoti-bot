using System.Threading.Tasks;

namespace Spoti_bot.Bot.HandleUpdate.Dto
{
    public interface IUpdateDtoService
    {
        Task<UpdateDto> Build(Telegram.Bot.Types.Update update);
    }
}