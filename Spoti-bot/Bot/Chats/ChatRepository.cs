using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Bot.Chats
{
    public class ChatRepository : BaseRepository<Chat>, IChatRepository
    {
        public ChatRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Chat).Name), "chats")
        {

        }
    }
}
