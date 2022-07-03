using Microsoft.WindowsAzure.Storage.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Bot.Chats
{
    public class ChatRepository : BaseRepository<Chat>, IChatRepository
    {
        public ChatRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Chat).Name), "chats")
        {

        }
    }
}
