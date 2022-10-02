using System.Threading.Tasks;

namespace SpotiBot.Bot.Users
{
    public interface IUserService
    {
        Task<User> SaveUser(User user, long chatId);
    }
}