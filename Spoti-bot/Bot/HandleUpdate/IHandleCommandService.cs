using Spoti_bot.Library;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.HandleUpdate
{
    public interface IHandleCommandService
    {
        bool IsAnyCommand(Message message);
        Task<BotResponseCode> TryHandleCommand(Message message, Chats.Chat chat);
    }
}