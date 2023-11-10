using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface IAuthorizationTokenService
    {
        Task<AuthorizationToken?> Get(long id);
        Task<AuthorizationToken> Upsert(AuthorizationToken token);
    }
}