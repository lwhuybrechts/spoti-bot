using Spoti_bot.Spotify.Data;

namespace Spoti_bot.Spotify
{
    public interface ISpotifyLinkHelper
    {
        bool HasAnySpotifyLink(string text);
        bool HasToSpotifyLink(string text);
        bool HasTrackIdLink(string text);
        string ParseToSpotifyLink(string text);
        Track ParseTrackId(string text);
    }
}
