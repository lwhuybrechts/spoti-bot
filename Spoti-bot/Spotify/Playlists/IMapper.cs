using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Playlists
{
    public interface IMapper
    {
        Playlist Map(FullPlaylist source);
    }
}
