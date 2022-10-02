using SpotifyAPI.Web;

namespace SpotiBot.Spotify.Playlists
{
    public interface IMapper
    {
        Playlist Map(FullPlaylist source);
    }
}
