using SpotiBot.Data.Models;
using System.Threading.Tasks;

namespace SpotiBot.Data.Repositories
{
    public interface IAuthorizationTokenRepository
    {
        Task<AuthorizationToken?> Get(long rowKey, string partitionKey = "");
        Task<AuthorizationToken> Upsert(AuthorizationToken item);
    }
}
