using Microsoft.WindowsAzure.Storage.Table;

namespace SpotiBot.Data.Models
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
