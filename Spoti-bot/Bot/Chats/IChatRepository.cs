using System.Threading.Tasks;

namespace SpotiBot.Bot.Chats
{
    public interface IChatRepository
    {
        Task<Chat> Get(long rowKey, string partitionKey = "");
        Task<Chat> Get(Chat item);
        Task Upsert(Chat item);
        Task Delete(Chat item);
    }
}