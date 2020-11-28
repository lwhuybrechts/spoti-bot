using AutoMapper;

namespace Spoti_bot.ApiModels
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<Spotify.Data.Tracks.Track, Track>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.AddedByTelegramUserId, options => options.MapFrom(source => source.AddedByTelegramUserId))
                .ForMember(destination => destination.AddedAt, options => options.MapFrom(source => source.CreatedAt))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<Bot.Data.Upvotes.Upvote, Upvote>()
                .ForMember(destination => destination.UserId, options => options.MapFrom(source => source.UserId))
                .ForMember(destination => destination.TrackId, options => options.MapFrom(source => source.TrackId))
                .ForMember(destination => destination.AddedAt, options => options.MapFrom(source => source.CreatedAt))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<Bot.Data.Users.User, User>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.FirstName, options => options.MapFrom(source => source.FirstName))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
