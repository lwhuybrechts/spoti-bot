using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Spotify.Data.User
{
    public class User : TableEntity
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
