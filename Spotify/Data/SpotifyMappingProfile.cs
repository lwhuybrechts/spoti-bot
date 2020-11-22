using AutoMapper;
using Spoti_bot.Spotify.Data.AuthorizationTokens;
using Spoti_bot.Spotify.Data.Tracks;
using SpotifyAPI.Web;
using System.Linq;

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
                .ForMember(destination => destination.FirstArtistName, options => options.MapFrom(source => source.Artists.FirstOrDefault().Name))
                .ForMember(destination => destination.AlbumName, options => options.MapFrom(source => source.Album.Name))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<string, Track>()
                // Make sure the RowKey is filled with the TrackId.
                .ForMember(destination => destination.RowKey, options => options.MapFrom(source => source))
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<Telegram.Bot.Types.User, Bot.Data.User.User>()
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