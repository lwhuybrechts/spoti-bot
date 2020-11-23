namespace Spoti_bot.Spotify.Interfaces
{
    public interface ISpotifyLinkHelper
    {
        bool HasAnySpotifyLink(string text);
        bool HasToSpotifyLink(string text);
        bool HasTrackIdLink(string text);
        string ParseToSpotifyLink(string text);
        string ParseTrackId(string text);
        string GetMarkdownLinkToPlaylist(string text = "");
    }
}
