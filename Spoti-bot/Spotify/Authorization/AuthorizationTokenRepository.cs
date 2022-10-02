using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Authorization
{
    public class AuthorizationTokenRepository : BaseRepository<AuthorizationToken>, IAuthorizationTokenRepository
    {
        public AuthorizationTokenRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(AuthorizationToken).Name), "authorizationtokens")
        {
        }
    }
}
