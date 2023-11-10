using SpotiBot.Data.Models;
using System.Threading.Tasks;

namespace SpotiBot.Data.Repositories
{
    public interface IChatRepository
    {
        Task<Chat> Get(long rowKey, string partitionKey = "");
        Task<Chat> Get(Chat item);
        Task<Chat> Upsert(Chat item);
        Task Delete(Chat item);
    }
}