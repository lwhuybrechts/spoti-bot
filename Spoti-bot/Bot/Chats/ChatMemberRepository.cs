using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Bot.Chats
{
    public class ChatMemberRepository : BaseRepository<ChatMember>, IChatMemberRepository
    {
        public ChatMemberRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(ChatMember).Name), null)
        {

        }
    }
}
