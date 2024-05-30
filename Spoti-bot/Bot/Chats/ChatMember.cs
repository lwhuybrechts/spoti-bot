using SpotiBot.Library;
using System.Runtime.Serialization;

namespace SpotiBot.Bot.Chats
{
    public class ChatMember : MyTableEntity
    {
        [IgnoreDataMember]
        public long ChatId
        {
            get { return long.Parse(PartitionKey); }
            set { PartitionKey = value.ToString(); }
        }

        [IgnoreDataMember]
        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }
    }
}
