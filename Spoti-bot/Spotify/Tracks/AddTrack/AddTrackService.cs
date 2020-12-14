using Spoti_bot.Bot;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Api;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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

        public async Task<BotResponseCode> TryAddTrackToPlaylist(Message message, Bot.Chats.Chat chat)
        {
            // Parse the trackId from the message.
            var newTrackId = await _spotifyLinkHelper.ParseTrackId(message.Text);
            if (string.IsNullOrEmpty(newTrackId))
                return BotResponseCode.NoAction;

            // Check if the track already exists in the playlist.
            if (await DoesExistInPlaylist(newTrackId, chat.PlaylistId))
            {
                await _sendMessageService.SendTextMessage(message, $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist(chat.PlaylistId, "playlist")}!");
                return BotResponseCode.TrackAlreadyExists;
            }

            var spotifyClient = await _spotifyClientFactory.Create(chat.AdminUserId);

            // We can't continue if we can't use the spotify api.
            if (spotifyClient == null)
            {
                await _sendMessageService.SendTextMessage(message, "Spoti-bot is not authorized to add this track to Spotify.");
                return BotResponseCode.NoAction;
            }

            // Get the track from the spotify api.
            var newTrack = await _spotifyClientService.GetTrack(spotifyClient, newTrackId);
            if (newTrack == null)
            {
                await _sendMessageService.SendTextMessage(message, $"Track not found in Spotify api :(");
                return BotResponseCode.NoAction;
            }

            await AddTrack(spotifyClient, message, newTrack, chat.PlaylistId);

            // Reply that the message has been added successfully.
            await SendReplyMessage(message, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(spotifyClient, newTrack);

            return BotResponseCode.TrackAddedToPlaylist;
        }

        private async Task<bool> DoesExistInPlaylist(string newTrackId, string playlistId)
        {
            return await _trackRepository.Get(newTrackId, playlistId) != null;
        }

        /// <summary>
        /// Add the track to the playlist.
        /// </summary>
        private async Task AddTrack(ISpotifyClient spotifyClient, Message message, Track newTrack, string playlistId)
        {
            newTrack.PlaylistId = playlistId;
            newTrack.CreatedAt = DateTimeOffset.UtcNow;
            newTrack.AddedByTelegramUserId = message.From.Id;

            // Add the track to the playlist.
            await _trackRepository.Upsert(newTrack);
            await _spotifyClientService.AddTrackToPlaylist(spotifyClient, newTrack, playlistId);
        }

        /// <summary>
        /// Reply when a track has been added to the playlist.
        /// </summary>
        private async Task SendReplyMessage(Message message, Track track)
        {
            var originalMessageId = message.MessageId;

            await _sendMessageService.SendTextMessage(
                message,
                _successResponseService.GetSuccessResponseText(message, track),
                replyToMessageId: originalMessageId,
                replyMarkup: _keyboardService.CreatePostedTrackResponseKeyboard());
        }
    }
}
