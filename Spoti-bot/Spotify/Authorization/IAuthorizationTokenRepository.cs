using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IAuthorizationTokenRepository
    {
        Task<AuthorizationToken> Get(long rowKey, string partitionKey = "");
        Task<AuthorizationToken> Upsert(AuthorizationToken item);
    }
}
