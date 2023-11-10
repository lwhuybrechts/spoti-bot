using Microsoft.WindowsAzure.Storage.Table;

namespace SpotiBot.Data.Models
{
    public class Chat : TableEntity
    {
        [IgnoreProperty]
        public long Id
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string Title { get; set; } = string.Empty;
        public string PlaylistId { get; set; } = string.Empty;
        public long AdminUserId { get; set; }
        public int Type { get; set; }
    }
}
