using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationTokenRepository : BaseRepository<AuthorizationToken>, IAuthorizationTokenRepository
    {
        public AuthorizationTokenRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(AuthorizationToken).Name), "test")
        {
        }
    }
}
