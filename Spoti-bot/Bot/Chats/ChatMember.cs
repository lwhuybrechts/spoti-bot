using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Bot.Chats
{
    public class ChatMember : TableEntity
    {
        public long ChatId
        {
            get { return long.Parse(PartitionKey); }
            set { PartitionKey = value.ToString(); }
        }

        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }
    }
}
