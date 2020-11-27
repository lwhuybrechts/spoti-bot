using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.Commands
{
    public interface ICommandsService
    {
        bool IsAnyCommand(Message message);
        Task<BotResponseCode> TryHandleCommand(Message message);
    }
}
