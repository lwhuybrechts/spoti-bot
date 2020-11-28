using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Bot.Users
{
    public class User : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
