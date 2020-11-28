using AutoMapper;
using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationMappingProfile : Profile
    {
        public AuthorizationMappingProfile()
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
        }
    }
}
