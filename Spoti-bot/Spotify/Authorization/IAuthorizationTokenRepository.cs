using System.Threading.Tasks;

namespace SpotiBot.Spotify.Authorization
{
    public interface IAuthorizationTokenRepository
    {
        Task<AuthorizationToken> Get(long rowKey, string partitionKey = "");
        Task<AuthorizationToken> Upsert(AuthorizationToken item);
    }
}
