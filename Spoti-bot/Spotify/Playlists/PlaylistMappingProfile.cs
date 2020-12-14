using AutoMapper;
using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Playlists
{
    public class PlaylistMappingProfile : Profile
    {
        public PlaylistMappingProfile()
        {
            // All properties are explicitly mapped to improve traceability, non explicitly mapped properties are ignored.

            CreateMap<FullPlaylist, Playlist>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.Id))
                .ForMember(destination => destination.Name, options => options.MapFrom(source => source.Name))
                .ForAllOtherMembers(memberOptions => memberOptions.Ignore());
        }
    }
}
