using SpotiBot.Library.Enums;

namespace SpotiBot.Api.Bot.Chats
{
    public class ParsedChat
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public ChatType Type { get; set; }

        public ParsedChat(long id, string? title, ChatType type)
        {
            Id = id;
            Title = title;
            Type = type;
        }
    }
}
