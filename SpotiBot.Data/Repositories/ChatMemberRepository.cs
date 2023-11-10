using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class ChatMemberRepository : BaseRepository<ChatMember>, IChatMemberRepository
    {
        public ChatMemberRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(ChatMember).Name), "")
        {

        }
    }
}
