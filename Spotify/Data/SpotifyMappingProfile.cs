using AutoMapper;
using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Data
{
    public class SpotifyMappingProfile : Profile
    {
        public SpotifyMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<AuthorizationToken, AuthorizationCodeTokenResponse>()
                .ForMember(destination => destination.AccessToken, options => options.MapFrom(source => source.AccessToken))
                .ForMember(destination => destination.TokenType, options => options.MapFrom(source => source.TokenType))
                .ForMember(destination => destination.ExpiresIn, options => options.MapFrom(source => source.ExpiresIn))
                .ForMember(destination => destination.Scope, options => options.MapFrom(source => source.Scope))
                .ForMember(destination => destination.RefreshToken, options => options.MapFrom(source => source.RefreshToken))
                .ForMember(destination => destination.CreatedAt, options => options.MapFrom(source => source.CreatedAt))
                .ForMember(destination => destination.IsExpired, options => options.MapFrom(source => source.IsExpired))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<AuthorizationCodeTokenResponse, AuthorizationToken>()
                .ForMember(destination => destination.AccessToken, options => options.MapFrom(source => source.AccessToken))
                .ForMember(destination => destination.TokenType, options => options.MapFrom(source => source.TokenType))
                .ForMember(destination => destination.ExpiresIn, options => options.MapFrom(source => source.ExpiresIn))
                .ForMember(destination => destination.Scope, options => options.MapFrom(source => source.Scope))
                .ForMember(destination => destination.RefreshToken, options => options.MapFrom(source => source.RefreshToken))
                .ForMember(destination => destination.CreatedAt, options => options.MapFrom(source => source.CreatedAt))
                .ForMember(destination => destination.IsExpired, options => options.MapFrom(source => source.IsExpired))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<FullTrack, Track>()
                // Make sure the RowKey is filled with the TrackId.
                .ForMember(destination => destination.RowKey, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<string, Track>()
                // Make sure the RowKey is filled with the TrackId.
                .ForMember(destination => destination.RowKey, options => options.MapFrom(source => source))
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}