using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Bot.Chats
{
    public class ChatMemberRepository : BaseRepository<ChatMember>, IChatMemberRepository
    {
        public ChatMemberRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(ChatMember).Name), null)
        {

        }
    }
}
