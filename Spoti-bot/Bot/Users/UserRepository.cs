using Azure.Data.Tables;
using SpotiBot.Library;

namespace SpotiBot.Bot.Users
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(TableServiceClient tableServiceClient)
            : base(tableServiceClient.GetTableClient(typeof(User).Name), "users")
        {

        }
    }
}
