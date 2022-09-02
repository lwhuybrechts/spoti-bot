using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public interface IUserRepository
    {
        Task<User> Get(long rowKey, string partitionKey = "");
        Task<User> Get(User item);
        Task<List<User>> GetAll();
        Task<User> Upsert(User item);
        Task Delete(User item);
    }
}
