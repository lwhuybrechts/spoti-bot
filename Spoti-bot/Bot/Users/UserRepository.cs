using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Library;

namespace SpotiBot.Bot.Users
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(User).Name), "users")
        {

        }
    }
}
