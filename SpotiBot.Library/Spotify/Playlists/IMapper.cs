using SpotiBot.Library.BusinessModels.Spotify;
using SpotifyAPI.Web;

namespace SpotiBot.Library.Spotify.Playlists
{
    public interface IMapper
    {
        Playlist? Map(FullPlaylist source);
    }
}
