using System;

namespace Spoti_bot.ApiModels
{
    public class Upvote
    {
        public string TrackId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }
    }
}
