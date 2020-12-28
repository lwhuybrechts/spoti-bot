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
        
        /// <summary>
        /// Used int since Azure Table Storage doesn't support enums.
        /// </summary>
        public int TypeValue { get; set; }

        [IgnoreProperty]
        public ChatType Type {
            get { return (ChatType)TypeValue; }
            set { TypeValue = (int)value; }
        }
    }
}
