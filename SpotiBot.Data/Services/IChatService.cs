using SpotiBot.Library.BusinessModels.Bot;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface IChatService
    {
        Task<Chat?> Get(long id);
        Task<Chat> Save(Chat chat);
    }
}