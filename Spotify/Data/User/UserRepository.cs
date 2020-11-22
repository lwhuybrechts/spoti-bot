using Microsoft.Azure.Cosmos.Table;
using Spoti_bot.Library;

namespace Spoti_bot.Spotify.Data.User
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(User).Name), "users")
        {

        }
    }
}
