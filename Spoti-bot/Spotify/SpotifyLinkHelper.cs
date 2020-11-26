using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Interfaces;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        /// Generate a markdown text link to the playlist.
        /// </summary>
        /// <param name="text">The clickable text for our link. Defaults to the name of the playlist.</param>
        public string GetMarkdownLinkToPlaylist(string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = _playlistOptions.Name;

            return $"[{text}]({PlaylistBaseUri}{_playlistOptions.Id})";
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
        /// Get a trackId from the text.
        /// </summary>
        /// <param name="text">The text to get the trackId from.</param>
        public async Task<string> ParseTrackId(string text)
        {
            // Check for a "classic" spotify url.
            if (HasTrackIdLink(text))
                return ParseTrackIdLink(text);

            // Check for a "linkto" spotify url.
            if (HasToSpotifyLink(text))
            {
                var linkToUri = ParseToSpotifyLink(text);

                // Get the trackUri from the linkToUri.
                var trackUri = await RequestTrackUri(linkToUri);

                if (HasTrackIdLink(trackUri))
                    return ParseTrackIdLink(trackUri);
            }

            // TODO: this crashes when a LinkTo url to something other than a track is posted.

            // This should not happen, so log it to Sentry.
            throw new TrackIdNullException();
        }

        /// <summary>
        /// Do a http GET request to a linkto uri to find the trackId.
        /// </summary>
        /// <param name="linkToUri">The uri to do a http GET request to.</param>
        private async Task<string> RequestTrackUri(string linkToUri)
        {
            // TODO: reuse httpclient from startup.
            var client = new System.Net.Http.HttpClient();

            // TODO: currently we get a badrequest, but the trackUri we're looking for is in it's request.
            // This feels pretty hacky, not sure if it will keep working in the future.
            var response = await client.GetAsync(linkToUri);
            
            var trackUri = response?.RequestMessage?.RequestUri?.AbsoluteUri ?? "";

            return trackUri;
        }

        /// <summary>
        /// Check if the text contains an url with a trackId.
        /// </summary>
        /// <param name="text">The text to check.</param>
        private bool HasTrackIdLink(string text)
        {
            return _trackRegex.Match(text).Success;
        }

        /// <summary>
        /// Parse the trackId from the text.
        /// </summary>
        /// <param name="text">The text to parse the trackId from.</param>
        private string ParseTrackIdLink(string text)
        {
            return _trackRegex.Match(text).Groups[3].Value;
        }

        /// <summary>
        /// Check if the text contains a tospotify url.
        /// </summary>
        /// <param name="text">The text to check.</param>
        private bool HasToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Success;
        }

        /// <summary>
        /// Parse the tospotify url from the text.
        /// </summary>
        /// <param name="text">The text to parse the url from.</param>
        private string ParseToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Value;
        }
    }
}