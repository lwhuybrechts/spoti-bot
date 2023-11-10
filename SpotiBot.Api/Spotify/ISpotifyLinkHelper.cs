using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify
{
    public interface ISpotifyLinkHelper
    {
        string GetMarkdownLinkToPlaylist(string playlistId, string text);
        string GetLinkToPlaylist(string playlistId);
        string GetLinkToTrack(string trackId);
        bool HasAnyTrackLink(string text);
        Task<string> ParsePlaylistId(string text);
        Task<string> ParseTrackId(string text);
    }
}
