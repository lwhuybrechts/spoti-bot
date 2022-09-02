using Spoti_bot.Bot;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
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
        private readonly IReplyMessageService _replyMessageService;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVoteService _voteService;
        private readonly IKeyboardService _keyboardService;

        public AddTrackService(
            ISendMessageService sendMessageService,
            IReplyMessageService successResponseService,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            ITrackRepository trackRepository,
            IUserRepository userRepository,
            IVoteService voteService,
            IKeyboardService keyboardService)
        {
            _sendMessageService = sendMessageService;
            _replyMessageService = successResponseService;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _trackRepository = trackRepository;
            _userRepository = userRepository;
            _voteService = voteService;
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
                await SendExistingTrackReplyMessage(updateDto, existingTrackInPlaylist);
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
            await SendSuccessfullyAddedReplyMessage(updateDto, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(spotifyClient, newTrack);

            return BotResponseCode.TrackAddedToPlaylist;
        }

        /// <summary>
        /// Add the track to the playlist.
        /// </summary>
        private async Task AddTrack(ISpotifyClient spotifyClient, User user, Track newTrack, string playlistId)
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
            var addedByUser = await _userRepository.Get(track.AddedByTelegramUserId);
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
