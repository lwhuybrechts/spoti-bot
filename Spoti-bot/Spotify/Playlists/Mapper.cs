using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Playlists
{
    public class Mapper : IMapper
    {
        public Playlist Map(FullPlaylist source) => new()
        {
            Id = source.Id,
            Name = source.Name
        };
    }
}
