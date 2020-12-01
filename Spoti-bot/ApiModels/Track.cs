using System;

namespace Spoti_bot.ApiModels
{
    public class Track
    {
        public string Id { get; set; }
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }
    }
}
