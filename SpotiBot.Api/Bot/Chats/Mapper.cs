using SpotiBot.Library.Enums;
using System;

namespace SpotiBot.Api.Bot.Chats
{
    public class Mapper : IMapper
    {
        public ParsedChat? Map(Telegram.Bot.Types.Chat? source) =>
            source == null ? null : new ParsedChat(
                source.Id,
                source.Title,
                MapChatType(source.Type)
            );

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
