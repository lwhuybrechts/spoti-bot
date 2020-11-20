using AutoMapper;
using Spoti_bot.Spotify.Data;
using System.Text.RegularExpressions;

namespace Spoti_bot.Spotify
{
    public class SpotifyLinkHelper : ISpotifyLinkHelper
    {
        public const string PlaylistBaseUri = "https://open.spotify.com/playlist/";
        public const string TrackInlineBaseUri = "spotify:track:";

        private static readonly Regex _trackRegex = new Regex("(https?://(open|play).spotify.com/track/|spotify:track:)([\\w\\d]+)");
        private static readonly Regex _linkToRegex = new Regex("(https?://link.tospotify.com/)([\\w\\d]+)");

        private readonly IMapper _mapper;

        public SpotifyLinkHelper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public bool HasAnySpotifyLink(string text)
        {
            return HasTrackIdLink(text) || HasToSpotifyLink(text);
        }

        public bool HasTrackIdLink(string text)
        {
            return _trackRegex.Match(text).Success;
        }

        public Track ParseTrackId(string text)
        {
            var trackId = _trackRegex.Match(text).Groups[3].Value;

            return _mapper.Map<Track>(trackId);
        }

        public bool HasToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Success;
        }

        public string ParseToSpotifyLink(string text)
        {
            return _linkToRegex.Match(text).Value;
        }
    }
}
