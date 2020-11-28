using AutoMapper;
using Spoti_bot.Spotify.Tracks.SyncHistory;
using SpotifyAPI.Web;
using System.Linq;

namespace Spoti_bot.Spotify.Tracks
{
    public class TrackMappingProfile : Profile
    {
        public TrackMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<FullTrack, Track>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.Name))
                .ForMember(destination => destination.FirstArtistName, options => options.MapFrom(source => source.Artists.FirstOrDefault().Name))
                .ForMember(destination => destination.AlbumName, options => options.MapFrom(source => source.Album.Name))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<string, Track>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());

            CreateMap<TrackAdded, Track>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.TrackId))
                .ForMember(destination => destination.AddedByTelegramUserId, options => options.MapFrom(source => source.FromId))
                .ForMember(destination => destination.CreatedAt, options => options.MapFrom(source => source.CreatedAt))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
