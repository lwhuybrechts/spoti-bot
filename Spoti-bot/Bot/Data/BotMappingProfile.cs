using AutoMapper;

namespace Spoti_bot.Bot.Data
{
    public class BotMappingProfile : Profile
    {
        public BotMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<Telegram.Bot.Types.User, Users.User>()
                // Make sure the RowKey is filled with the Telegram User Id.
                .ForMember(destination => destination.RowKey, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.FirstName, options => options.MapFrom(source => source.FirstName))
                .ForMember(destination => destination.LastName, options => options.MapFrom(source => source.LastName))
                .ForMember(destination => destination.UserName, options => options.MapFrom(source => source.Username))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
