﻿using Microsoft.WindowsAzure.Storage.Table;

namespace SpotiBot.Bot.Users
{
    public class User : TableEntity
    {
        [IgnoreProperty]
        public long Id
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        [IgnoreProperty]
        public string? LanguageCode { get; set; }
    }
}
