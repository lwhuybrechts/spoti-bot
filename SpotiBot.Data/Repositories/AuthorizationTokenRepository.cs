using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class AuthorizationTokenRepository : BaseRepository<AuthorizationToken>, IAuthorizationTokenRepository
    {
        public AuthorizationTokenRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(AuthorizationToken).Name), "authorizationtokens")
        {
        }
    }
}
