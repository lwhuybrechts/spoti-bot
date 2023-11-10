using SpotiBot.Api.Bot;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Data.Services;
using SpotiBot.Library.Enums;
using SpotiBot.Library.Spotify.Api;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;
using Track = SpotiBot.Library.BusinessModels.Spotify.Track;

namespace SpotiBot.Api.Spotify.Tracks.AddTrack
{
    public class AddTrackService : IAddTrackService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly IReplyMessageService _replyMessageService;
        private readonly IAuthService _authService;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackService _trackService;
        private readonly IUserService _userService;
        private readonly IKeyboardService _keyboardService;
        private readonly Bot.Votes.IVoteService _voteService;

        public AddTrackService(
            ISendMessageService sendMessageService,
            IReplyMessageService successResponseService,
            IAuthService authService,
            ISpotifyClientService spotifyClientService,
            ITrackService trackService,
            IUserService userService,
            IKeyboardService keyboardService,
            Bot.Votes.IVoteService voteService)
        {
            _sendMessageService = sendMessageService;
            _replyMessageService = successResponseService;
            _authService = authService;
            _spotifyClientService = spotifyClientService;
            _trackService = trackService;
            _userService = userService;
            _keyboardService = keyboardService;
            _voteService = voteService;
        }

        public async Task<BotResponseCode> TryAddTrackToPlaylist(UpdateDto updateDto)
        {
            if (string.IsNullOrEmpty(updateDto.ParsedTrackId))
                return BotResponseCode.NoAction;

            var existingTrackInPlaylist = await _trackService.Get(updateDto.ParsedTrackId, updateDto.Chat.PlaylistId);

            // Check if the track already exists in the playlist.
            if (existingTrackInPlaylist != null)
            {
                await SendExistingTrackReplyMessage(updateDto, existingTrackInPlaylist);
                return BotResponseCode.TrackAlreadyExists;
            }

            var spotifyClient = await _authService.GetClient(updateDto.Chat.AdminUserId);
            // We can't continue if we can't use the spotify api.
            if (spotifyClient == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, "Spoti-bot is not authorized to add this track to the Spotify playlist.");
                return BotResponseCode.NoAction;
            }

            // Get the track from the spotify api.
            var track = await _spotifyClientService.GetTrack(spotifyClient, updateDto.ParsedTrackId, updateDto.Chat.PlaylistId, updateDto.ParsedUser.Id, DateTimeOffset.UtcNow, TrackState.AddedToPlaylist);
            if (track == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, $"Track not found in Spotify api :(");
                return BotResponseCode.NoAction;
            }

            await AddTrack(spotifyClient, track);

            // Reply that the message has been added successfully.
            await SendSuccessfullyAddedReplyMessage(updateDto, track);

            // Add the track to my queue.
            try
            {
                await _spotifyClientService.AddToQueue(spotifyClient, track);
            }
            catch (APIException exception) when (exception.Message == "Restricted device")
            {
                // Ignore.
            }

            return BotResponseCode.TrackAddedToPlaylist;
        }

        /// <summary>
        /// Add the track to the playlist.
        /// </summary>
        private async Task AddTrack(ISpotifyClient spotifyClient, Track track)
        {
            await _trackService.Upsert(track);
            await _spotifyClientService.AddTrackToPlaylist(spotifyClient, track.Id, track.PlaylistId);
        }

        /// <summary>
        /// Reply when a track has been added to the playlist.
        /// </summary>
        private Task SendSuccessfullyAddedReplyMessage(UpdateDto updateDto, Track track)
        {
            return _sendMessageService.SendTextMessage(
                updateDto.Chat.Id,
                _replyMessageService.GetSuccessReplyMessage(updateDto, track),
                replyToMessageId: int.Parse(updateDto.ParsedUpdateId),
                replyMarkup: _keyboardService.CreatePostedTrackResponseKeyboard());
        }

        /// <summary>
        /// Reply when the track already existed in the playlist.
        /// </summary>
        private async Task SendExistingTrackReplyMessage(UpdateDto updateDto, Track track)
        {
            var addedByUser = await _userService.Get(track.AddedByTelegramUserId);
            var replyText = _replyMessageService.GetExistingTrackReplyMessage(updateDto, track, addedByUser);
            (var replyTextWithVotes, var keyboard) = await _voteService.UpdateTextAndKeyboard(replyText, _keyboardService.CreatePostedTrackResponseKeyboard(), track);

            await _sendMessageService.SendTextMessage(
                updateDto.Chat.Id,
                replyTextWithVotes,
                replyToMessageId: int.Parse(updateDto.ParsedUpdateId),
                replyMarkup: keyboard);
        }
    }
}
