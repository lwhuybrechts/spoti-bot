using Microsoft.WindowsAzure.Storage.Table;
using SpotiBot.Data.Models;

namespace SpotiBot.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(CloudTableClient cloudTableClient)
            : base(cloudTableClient.GetTableReference(typeof(User).Name), "users")
        {

        }
    }
}
