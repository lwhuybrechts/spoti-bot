using SpotiBot.Library.Enums;

namespace SpotiBot.Library.BusinessModels.Bot
{
    public class Chat
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PlaylistId { get; set; } = string.Empty;
        public long AdminUserId { get; set; }
        public ChatType Type { get; set; }

        public Chat(long id, string? title, string? playlistId, long adminUserId, ChatType type)
        {
            Id = id;
            AdminUserId = adminUserId;
            Type = type;

            if (title != null)
                Title = title;

            if (playlistId != null)
                PlaylistId = playlistId;
        }
    }
}
