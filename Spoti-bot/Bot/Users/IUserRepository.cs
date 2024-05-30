using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Bot.Users
{
    public interface IUserRepository
    {
        Task<User> Get(long rowKey, string partitionKey = "");
        Task<User> Get(User item);
        Task<List<User>> GetAll();
        Task Upsert(User item);
        Task Delete(User item);
    }
}
