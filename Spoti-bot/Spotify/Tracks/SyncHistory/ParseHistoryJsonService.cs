using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncHistory
{
    public class ParseHistoryJsonService : IParseHistoryJsonService
    {
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public ParseHistoryJsonService(ISpotifyLinkHelper spotifyLinkHelper)
        {
            _spotifyLinkHelper = spotifyLinkHelper;
        }

        /// <summary>
        /// Parse trackIds from the json string and create TrackAdded objects with them.
        /// </summary>
        /// <param name="jsonString">The json string to parse from.</param>
        /// <param name="dateTimeKind">The kind of dates that are in the json messages.</param>
        public async Task<List<TrackAdded>> ParseTracks(string jsonString, DateTimeKind dateTimeKind)
        {
            var jObject = JObject.Parse(jsonString);

            // We expect a json object with a messages key.
            if (!jObject.ContainsKey("messages"))
                return default;

            // We cant deserialize directly to Messages since the date format is different, so use JObjects instead.
            var messages = jObject.SelectToken("messages").Values<JObject>();

            // Get the tracks from all messages.
            var tracks = new List<TrackAdded>();
            foreach (var message in messages)
                tracks.AddRange(await GetTracksFromMessage(message, dateTimeKind));

            return tracks;
        }

        /// <summary>
        /// Get all texts from a message and create TrackAdded objects with them.
        /// </summary>
        private async Task<List<TrackAdded>> GetTracksFromMessage(JObject message, DateTimeKind dateTimeKind)
        {
            if (!message.ContainsKey("type") ||
                // Only check objects with the type message.
                message.SelectToken("type").Value<string>() != "message" ||
                !message.ContainsKey("from_id") ||
                !message.ContainsKey("date") ||
                !message.ContainsKey("text"))
                return new List<TrackAdded>();

            // A message can have multiple text strings.
            var texts = ParseTexts(message.SelectToken("text"));

            var fromId = message.SelectToken("from_id").Value<long>();
            var createdAtLocal = message.SelectToken("date").Value<DateTime>();
            var createdAt = DateTime.SpecifyKind(createdAtLocal, dateTimeKind);

            return await CreateTracksFromTexts(texts, fromId, createdAt);
        }

        /// <summary>
        /// Checks if texts have trackIds in them, and if so creates TrackAdded objects with them.
        /// </summary>
        private async Task<List<TrackAdded>> CreateTracksFromTexts(List<string> texts, long fromId, DateTime createdAt)
        {
            var tracks = new List<TrackAdded>();
            
            foreach (var text in texts)
            {
                if (!_spotifyLinkHelper.HasAnySpotifyLink(text))
                    continue;

                var trackId = await _spotifyLinkHelper.ParseTrackId(text);
                if (string.IsNullOrEmpty(trackId))
                    continue;

                tracks.Add(new TrackAdded(fromId, createdAt, trackId));
            }

            return tracks;
        }

        /// <summary>
        /// Parse the text property to a list of strings.
        /// </summary>
        private List<string> ParseTexts(JToken textToken)
        {
            // Text can be a string.
            if (textToken.GetType() == typeof(JValue))
            {
                return new List<string>
                {
                    textToken.Value<string>()
                };
            }
            // Text can be an object, in that case it should have a text property.
            else if (textToken.GetType() == typeof(JObject))
            {
                return new List<string>
                {
                    textToken.SelectToken("text").Value<string>()
                };
            }
            // Text can be an array of strings and/or objects with a text property.
            else if (textToken.GetType() == typeof(JArray))
            {
                var texts = new List<string>();
                foreach (var textTokenInArray in textToken)
                    // Call this function recursively to parse the texts.
                    texts.AddRange(ParseTexts(textTokenInArray));
                return texts;
            }

            throw new InvalidCastException($"TextToken did not have an excepted format: {textToken}");
        }
    }
}
