using AutoMapper;
using Telegram.Bot.Types.InlineQueryResults;

namespace Spoti_bot.Bot.Users
{
    class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<Telegram.Bot.Types.User, User>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.FirstName, options => options.MapFrom(source => source.FirstName))
                .ForMember(destination => destination.LastName, options => options.MapFrom(source => source.LastName))
                .ForMember(destination => destination.UserName, options => options.MapFrom(source => source.Username))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<User, InlineQueryResultArticle>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Title, options => options.MapFrom(source => source.FirstName))
                .ForMember(destination => destination.InputMessageContent, options => options.MapFrom(source => new InputTextMessageContent(source.FirstName)))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
