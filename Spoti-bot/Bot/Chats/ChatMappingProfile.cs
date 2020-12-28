using AutoMapper;
using Spoti_bot.Bot.Chats;
using System;

namespace Spoti_bot.Bot.Users
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<Telegram.Bot.Types.Chat, Chat>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Title, options => options.MapFrom(source => source.Title))
                .ForMember(destination => destination.Type, options => options.MapFrom(source => MapChatType(source.Type)))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }

        private ChatType MapChatType(Telegram.Bot.Types.Enums.ChatType chatType)
        {
            return chatType switch
            {
                Telegram.Bot.Types.Enums.ChatType.Private => ChatType.Private,
                Telegram.Bot.Types.Enums.ChatType.Group => ChatType.Group,
                Telegram.Bot.Types.Enums.ChatType.Channel => ChatType.Channel,
                Telegram.Bot.Types.Enums.ChatType.Supergroup => ChatType.Supergroup,
                _ => throw new NotImplementedException($"{nameof(ChatType)} {chatType} is not implemented in the {nameof(ChatMappingProfile)}.")
            };
        }
    }
}
