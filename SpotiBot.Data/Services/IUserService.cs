using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface IUserService
    {
        Task<User?> Get(long id);
        Task<List<User>> Get(List<long> ids);
        Task<List<User>> GetAllExcept(long id);
        Task<User> SaveUser(User user, long chatId);
    }
}