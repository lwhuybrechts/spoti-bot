using SpotiBot.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Repositories
{
    public interface IChatMemberRepository
    {
        Task<ChatMember?> Get(long rowKey, string partitionKey = "");
        Task<List<ChatMember>> GetAllByPartitionKey(string partitionKey);
        Task<ChatMember> Upsert(ChatMember item);
        Task Delete(List<ChatMember> items);
    }
}