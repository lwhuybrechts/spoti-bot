using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpotiBot.Api.Spotify.Tracks.AddTrack
{
    public class ReplyMessageService : IReplyMessageService
    {
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;

        public ReplyMessageService(ISpotifyLinkHelper spotifyLinkHelper)
        {
            _spotifyLinkHelper = spotifyLinkHelper;
        }

        /// <summary>
        /// When a track is added to the playlist, get a nice response text to send to the chat.
        /// </summary>
        /// <param name="updateDto">The update that was sent.</param>
        /// <param name="track">The track that was added.</param>
        /// <returns>A text to reply to the chat with.</returns>
        public string GetSuccessReplyMessage(UpdateDto updateDto, Track track)
        {
            var successMessage = GetTrackInfo(track) +
                $"Track added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(track.PlaylistId, "playlist")}!";

            // TODO: inject random wrapper so this service is testable.
            var random = new Random();
            if (!ShouldAddAwesomeResponse(random))
                return successMessage;

            var firstName = updateDto.User?.FirstName;

            if (string.IsNullOrEmpty(firstName))
                return successMessage;

            return GetRandomAwesomeResponse(random, successMessage, firstName, track);
        }

        /// <summary>
        /// When a track that was sent was already added to the playlist, get a nice response text to send to the chat.
        /// </summary>
        /// <param name="updateDto">The update that was sent.</param>
        /// <param name="track">The track that was added.</param>
        /// <param name="addedByUser">The user that already added the track.</param>
        /// <returns>A text to reply to the chat with.</returns>
        public string GetExistingTrackReplyMessage(UpdateDto updateDto, Track track, User addedByUser)
        {
            var trackInfo = GetTrackInfo(track);

            var userText = string.Empty;
            if (addedByUser != null)
                userText = $" by {addedByUser.FirstName}";

            var dateText = string.Empty;
            
            // TODO: make the culture configurable.
            var cultureIfo = new CultureInfo(updateDto.ParsedUser.LanguageCode);
            dateText = $" on {track.CreatedAt.ToString("d", cultureIfo)}";

            if (track.State == TrackState.RemovedByDownvotes)
                return $"{trackInfo}This track was previously posted{userText}, but it was downvoted and removed from the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Chat.PlaylistId, "playlist")}.";
            else
                return $"{trackInfo}This track was already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Chat.PlaylistId, "playlist")}{userText}{dateText}!";
        }

        /// <summary>
        /// Sometimes we want to add an awesome response to the successText, but not always since that might get lame.
        /// </summary>
        private static bool ShouldAddAwesomeResponse(Random random)
        {
            // 1 in 5 chance we add an awesome response.
            return random.Next(0, 5) == 0;
        }

        private static string GetRandomAwesomeResponse(Random random, string successMessage, string firstName, Track track)
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
                $"This shit slaps, {firstName}!",
                $"This track is on and crackin, {firstName}!",
                "BOUNCE!",
                "Ouleh papi"
            };

            if (!string.IsNullOrEmpty(track.FirstArtistName))
                responses.Add($"Always love me some {track.FirstArtistName}!");

            if (!string.IsNullOrEmpty(track.AlbumName))
                responses.Add($"Also check out it's album, {track.AlbumName}.");

            return $"{successMessage} {responses[random.Next(0, responses.Count)]}";
        }

        private static string GetTrackInfo(Track track)
        {
            return $"*{track.Name}*\n" +
                $"{track.FirstArtistName} · {track.AlbumName}\n\n";
        }
    }
}
