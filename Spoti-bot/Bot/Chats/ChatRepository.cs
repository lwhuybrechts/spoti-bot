using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Bot.Chats
{
    public class ChatRepository : BaseRepository<Chat>, IChatRepository
    {
        public ChatRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(Chat).Name), "chats")
        {

        }
    }
}
