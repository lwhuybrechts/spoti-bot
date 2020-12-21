using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Bot.Chats
{
    public class ChatMember : TableEntity
    {
        [IgnoreProperty]
        public long ChatId
        {
            get { return long.Parse(PartitionKey); }
            set { PartitionKey = value.ToString(); }
        }

        [IgnoreProperty]
        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }
    }
}
