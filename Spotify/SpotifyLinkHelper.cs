using Microsoft.Extensions.Options;
using Spoti_bot.Library.Options;
using System.Text.RegularExpressions;

namespace Spoti_bot.Spotify
{
    public class SpotifyLinkHelper : ISpotifyLinkHelper
    {
        private const string PlaylistBaseUri = "https://open.spotify.com/playlist/";
        
        private static readonly Regex _trackRegex = new Regex("(https?://(open|play).spotify.com/track/|spotify:track:)([\\w\\d]+)");
        private static readonly Regex _linkToRegex = new Regex("(https?://link.tospotify.com/)([\\w\\d]+)");

        private readonly PlaylistOptions _playlistOptions;

        public SpotifyLinkHelper(IOptions<PlaylistOptions> playlistOptions)
        {
            _playlistOptions = playlistOptions.Value;
        }

        /// <summary>
        /// Check if the text contains an url with a trackId, or a tospotify url.
        /// </summary>
        /// <param name="text">The text to check.</param>
        public bool HasAnySpotifyLink(string text)
        {
            return HasTrackIdLink(text) || HasToSpotifyLink(text);
        }

        /// <summary>
        /// Check if the text contains an url with a trackId.
        /// </summary>
        /// <param name="text">The text to check.</param>
        public bool HasTrackIdLink(string text)
        {
            return _trackRegex.Match(text).Success;
        }

        /// <summary>
        /// Parse the trackId from the text.
        /// </summary>
        /// <param name="text">The text to parse the trackId from.</param>
        public string ParseTrackId(string text)
        {
            return _trackRegex.Match(text).Groups[3].Value;
        }

        /// <summary>
        /// Check if the text contains a tospotify url.
        /// </summary>
        /// <param name="text">The text to check.</param>
        public bool HasToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Success;
        }

        /// <summary>
        /// Parse the tospotify url from the text.
        /// </summary>
        /// <param name="text">The text to parse the url from.</param>
        public string ParseToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Value;
        }

        /// <summary>
        /// Generate a markdown text link to the playlist.
        /// </summary>
        /// <param name="text">The clickable text for our link. Defaults to the name of the playlist.</param>
        public string GetMarkdownLinkToPlaylist(string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = _playlistOptions.Name;

            return $"[{text}]({PlaylistBaseUri}{_playlistOptions.Id})";
        }
    }
}