using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Bot.Chats
{
    public interface IChatMemberRepository
    {
        Task<ChatMember> Get(long rowKey, string partitionKey = "");
        Task<List<ChatMember>> GetAllByPartitionKey(string partitionKey);
        Task<ChatMember> Upsert(ChatMember item);
        Task Delete(List<ChatMember> items);
    }
}