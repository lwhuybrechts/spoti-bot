using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Spotify.Authorization
{
    public class AuthorizationTokenRepository : BaseRepository<AuthorizationToken>, IAuthorizationTokenRepository
    {
        public AuthorizationTokenRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(AuthorizationToken).Name), "authorizationtokens")
        {
        }
    }
}
