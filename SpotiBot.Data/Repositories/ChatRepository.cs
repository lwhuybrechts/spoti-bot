using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class ChatRepository : BaseRepository<Chat>, IChatRepository
    {
        public ChatRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(Chat).Name), "chats")
        {

        }
    }
}
