using AutoMapper;
using Spoti_bot.Bot.Chats;

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
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
