using System;

namespace Spoti_bot.Bot.Chats
{
    public class Mapper : IMapper
    {
        public Chat Map(Telegram.Bot.Types.Chat source) => new()
        {
            Id = source.Id,
            Title = source.Title,
            Type = MapChatType(source.Type)
        };

        private ChatType MapChatType(Telegram.Bot.Types.Enums.ChatType chatType)
        {
            return chatType switch
            {
                Telegram.Bot.Types.Enums.ChatType.Private => ChatType.Private,
                Telegram.Bot.Types.Enums.ChatType.Group => ChatType.Group,
                Telegram.Bot.Types.Enums.ChatType.Channel => ChatType.Channel,
                Telegram.Bot.Types.Enums.ChatType.Supergroup => ChatType.Supergroup,
                _ => throw new NotImplementedException($"{nameof(ChatType)} {chatType} is not implemented.")
            };
        }
    }
}
