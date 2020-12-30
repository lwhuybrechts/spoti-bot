using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify
{
    public class SpotifyLinkHelper : ISpotifyLinkHelper
    {
        private const string PlaylistBaseUri = "https://open.spotify.com/playlist/";
        private const string TrackBaseUri = "https://open.spotify.com/track/";

        private static readonly Regex _playlistRegex = new Regex("(https?://(open|play).spotify.com/playlist/|spotify:playlist:)([\\w\\d]+)");
        private static readonly Regex _trackRegex = new Regex("(https?://(open|play).spotify.com/track/|spotify:track:)([\\w\\d]+)");
        private static readonly Regex _linkToRegex = new Regex("(https?://link.tospotify.com/)([\\w\\d]+)");

        /// <summary>
        /// Generate a markdown text link to the playlist.
        /// </summary>
        /// <param name="text">The clickable text for our link.</param>
        public string GetMarkdownLinkToPlaylist(string playlistId, string text)
        {
            return $"[{text}]({PlaylistBaseUri}{playlistId})";
        }

        public string GetLinkToPlaylist(string playlistId)
        {
            return $"{PlaylistBaseUri}{playlistId}";
        }

        public string GetLinkToTrack(string trackId)
        {
            return $"{TrackBaseUri}{trackId}";
        }

        /// <summary>
        /// Check if the text contains an url with a trackId, or a tospotify url.
        /// </summary>
        /// <param name="text">The text to check.</param>
        public bool HasAnyTrackLink(string text)
        {
            return HasTrackIdLink(text) || HasToSpotifyLink(text);
        }

        /// <summary>
        /// Get a playlistId from the text.
        /// </summary>
        /// <param name="text">The text to get the playlistId from.</param>
        public async Task<string> ParsePlaylistId(string text)
        {
            // Check for a "classic" spotify url.
            if (HasPlaylistIdLink(text))
                return ParsePlaylistIdLink(text);

            // Check for a "linkto" spotify url.
            if (HasToSpotifyLink(text))
            {
                var linkToUri = ParseToSpotifyLink(text);

                // Get the playlistUri from the linkToUri.
                var playlistUri = await RequestTrackUri(linkToUri);

                if (HasPlaylistIdLink(playlistUri))
                    return ParsePlaylistIdLink(playlistUri);
            }

            return string.Empty;
        }

        /// <summary>
        /// Get a trackId from the text.
        /// </summary>
        /// <param name="text">The text to get the trackId from.</param>
        public async Task<string> ParseTrackId(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

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

            return string.Empty;
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
        /// Check if the text contains an url with a playlistId.
        /// </summary>
        /// <param name="text">The text to check.</param>
        private bool HasPlaylistIdLink(string text)
        {
            return _playlistRegex.Match(text).Success;
        }

        /// <summary>
        /// Parse the playlistId from the text.
        /// </summary>
        /// <param name="text">The text to parse the playlistId from.</param>
        private string ParsePlaylistIdLink(string text)
        {
            return _playlistRegex.Match(text).Groups[3].Value;
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