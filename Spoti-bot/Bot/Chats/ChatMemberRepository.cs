using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Bot.Chats
{
    public class ChatMemberRepository : BaseRepository<ChatMember>, IChatMemberRepository
    {
        public ChatMemberRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(ChatMember).Name), null)
        {

        }
    }
}
