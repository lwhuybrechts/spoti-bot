using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Bot.Chats
{
    public class Chat : TableEntity
    {
        [IgnoreProperty]
        public long Id
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string Title { get; set; }
        public string PlaylistId { get; set; }
        public long AdminUserId { get; set; }
    }
}
