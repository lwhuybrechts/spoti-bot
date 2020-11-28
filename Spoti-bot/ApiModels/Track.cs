using System;

namespace Spoti_bot.ApiModels
{
    public class Track
    {
        public string Id { get; set; }
        public int AddedByTelegramUserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }
    }
}
