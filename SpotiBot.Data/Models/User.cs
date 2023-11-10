using Microsoft.WindowsAzure.Storage.Table;

namespace SpotiBot.Data.Models
{
    public class User : TableEntity
    {
        [IgnoreProperty]
        public long Id
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
