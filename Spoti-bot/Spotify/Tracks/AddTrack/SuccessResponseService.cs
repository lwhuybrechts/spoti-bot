using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public class SuccessResponseService : ISuccessResponseService
    {
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public SuccessResponseService(ISpotifyLinkHelper spotifyLinkHelper)
        {
            _spotifyLinkHelper = spotifyLinkHelper;
        }

        /// <summary>
        /// When a track is added to the playlist, get a nice response text to send to the chat.
        /// </summary>
        /// <param name="message">The message that contains the added trackId.</param>
        /// <param name="track">The track that was added.</param>
        /// <returns>A text to reply to the chat with.</returns>
        public string GetSuccessResponseText(Message message, Track track)
        {
            var successMessage = $"Track added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist("playlist")}!";

            // TODO: inject random wrapper so this service is testable.
            var random = new Random();
            if (!ShouldAddAwesomeResponse(random))
                return successMessage;

            var firstName = message?.From?.FirstName;

            if (string.IsNullOrEmpty(firstName))
                return successMessage;

            return GetRandomAwesomeResponse(random, successMessage, firstName, track);
        }

        /// <summary>
        /// Sometimes we want to add an awesome response to the successText, but not always since that might get lame.
        /// </summary>
        private static bool ShouldAddAwesomeResponse(Random random)
        {
            // 1 in 5 chance we add an awesome response.
            return random.Next(0, 5) == 0;
        }

        private string GetRandomAwesomeResponse(Random random, string successMessage, string firstName, Track track)
        {
            var responses = new List<string>
            {
                $"What an absolute banger, {firstName}!",
                $"Lit af, {firstName}!",
                $"Nice one {firstName}, thanks for sharing!",
                $"Dope-ass-beat, {firstName}!",
                $"This track is the bomb, {firstName}!",
                $"Thanks {firstName}, I like it a lot!",
                $"This track is ill af, {firstName}!",
                $"Neat-o, {firstName}!",
                $"Right on, {firstName}!",
                $"Oh my goodness, I love it {firstName}!",
                $"Ooooh yes, that is really swell {firstName}.",
                "BOUNCE!"
            };

            if (!string.IsNullOrEmpty(track.FirstArtistName))
                responses.Add($"Always love me some {track.FirstArtistName}!");

            if (!string.IsNullOrEmpty(track.AlbumName))
                responses.Add($"Also check out it's album, {track.AlbumName}.");

            return $"{successMessage} {responses[random.Next(0, responses.Count)]}";
        }
    }
}
