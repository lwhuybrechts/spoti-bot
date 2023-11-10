using SpotiBot.Library.Enums;

namespace SpotiBot.Library.BusinessModels.Bot
{
    public class Chat
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? PlaylistId { get; set; }
        public long AdminUserId { get; set; }
        public ChatType Type { get; set; }

        public Chat(long id, string? title, string? playlistId, long adminUserId, ChatType type)
        {
            Id = id;
            Title = title;
            PlaylistId = playlistId;
            AdminUserId = adminUserId;
            Type = type;
        }
    }
}
