﻿using Spoti_bot.Bot;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Api;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public class AddTrackService : IAddTrackService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISuccessResponseService _successResponseService;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly IKeyboardService _keyboardService;

        public AddTrackService(
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyTextHelper,
            ISuccessResponseService successResponseService,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            ITrackRepository trackRepository,
            IKeyboardService keyboardService)
        {
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyTextHelper;
            _successResponseService = successResponseService;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _trackRepository = trackRepository;
            _keyboardService = keyboardService;
        }

        public async Task<BotResponseCode> TryAddTrackToPlaylist(UpdateDto updateDto)
        {
            if (string.IsNullOrEmpty(updateDto.ParsedTrackId))
                return BotResponseCode.NoAction;

            var existingTrackInPlaylist = await _trackRepository.Get(updateDto.ParsedTrackId, updateDto.Chat.PlaylistId);

            // Check if the track already exists in the playlist.
            if (existingTrackInPlaylist != null)
            {
                string text;
                if (existingTrackInPlaylist.State == TrackState.RemovedByDownvotes)
                    text = $"This track was previously posted, but it was downvoted and removed from the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Chat.PlaylistId, "playlist")}.";
                else
                    text = $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(updateDto.Chat.PlaylistId, "playlist")}!";

                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, text);
                return BotResponseCode.TrackAlreadyExists;
            }

            var spotifyClient = await _spotifyClientFactory.Create(updateDto.Chat.AdminUserId);

            // We can't continue if we can't use the spotify api.
            if (spotifyClient == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, "Spoti-bot is not authorized to add this track to the Spotify playlist.");
                return BotResponseCode.NoAction;
            }

            // Get the track from the spotify api.
            var newTrack = await _spotifyClientService.GetTrack(spotifyClient, updateDto.ParsedTrackId);
            if (newTrack == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, $"Track not found in Spotify api :(");
                return BotResponseCode.NoAction;
            }

            await AddTrack(spotifyClient, updateDto.ParsedUser, newTrack, updateDto.Chat.PlaylistId);

            // Reply that the message has been added successfully.
            await SendReplyMessage(updateDto, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(spotifyClient, newTrack);

            return BotResponseCode.TrackAddedToPlaylist;
        }

        /// <summary>
        /// Add the track to the playlist.
        /// </summary>
        private async Task AddTrack(ISpotifyClient spotifyClient, Bot.Users.User user, Track newTrack, string playlistId)
        {
            newTrack.PlaylistId = playlistId;
            newTrack.CreatedAt = DateTimeOffset.UtcNow;
            newTrack.AddedByTelegramUserId = user.Id;
            newTrack.State = TrackState.AddedToPlaylist;

            // Add the track to the playlist.
            await _trackRepository.Upsert(newTrack);
            await _spotifyClientService.AddTrackToPlaylist(spotifyClient, newTrack);
        }

        /// <summary>
        /// Reply when a track has been added to the playlist.
        /// </summary>
        private async Task SendReplyMessage(UpdateDto updateDto, Track track)
        {
            await _sendMessageService.SendTextMessage(
                updateDto.Chat.Id,
                _successResponseService.GetSuccessResponseText(updateDto, track),
                replyToMessageId: int.Parse(updateDto.ParsedUpdateId),
                replyMarkup: _keyboardService.CreatePostedTrackResponseKeyboard());
        }
    }
}
