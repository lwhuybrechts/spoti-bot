using Spoti_bot.Bot.Upvotes;
using Spoti_bot.Library;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public class AddTrackService : IAddTrackService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISuccessResponseService _successResponseService;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly IUpvoteService _upvoteService;

        public AddTrackService(
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyTextHelper,
            ISuccessResponseService successResponseService,
            ISpotifyClientService spotifyClientService,
            ITrackRepository trackRepository,
            IUpvoteService upvoteService)
        {
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyTextHelper;
            _successResponseService = successResponseService;
            _spotifyClientService = spotifyClientService;
            _trackRepository = trackRepository;
            _upvoteService = upvoteService;
        }

        public async Task<BotResponseCode> TryAddTrackToPlaylist(Message message)
        {
            // Parse the trackId from the message.
            var newTrackId = await _spotifyLinkHelper.ParseTrackId(message.Text);
            if (string.IsNullOrEmpty(newTrackId))
                return BotResponseCode.NoAction;

            // Check if the track already exists in the playlist.
            if (await DoesExistInPlaylist(newTrackId))
            {
                await _sendMessageService.SendTextMessageAsync(message, $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist("playlist")}!");
                return BotResponseCode.TrackAlreadyExists;
            }

            // We can't continue if we can't use the spotify api.
            if (!await _spotifyClientService.HasClient())
            {
                await _sendMessageService.SendTextMessageAsync(message, "Spoti-bot is not authorized to add this track to Spotify.");
                return BotResponseCode.NoAction;
            }

            // Get the track from the spotify api.
            var newTrack = await _spotifyClientService.GetTrack(newTrackId);
            if (newTrack == null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"Track not found in Spotify api :(");
                return BotResponseCode.NoAction;
            }

            await AddTrack(message, newTrack);

            // Reply that the message has been added successfully.
            await SendReplyMessage(message, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(newTrack);

            return BotResponseCode.TrackAddedToPlaylist;
        }

        private async Task<bool> DoesExistInPlaylist(string newTrackId)
        {
            return await _trackRepository.Get(newTrackId) != null;
        }

        /// <summary>
        /// Add the track to the playlist.
        /// </summary>
        private async Task AddTrack(Message message, Track newTrack)
        {
            newTrack.CreatedAt = DateTimeOffset.UtcNow;
            newTrack.AddedByTelegramUserId = message.From.Id;

            // Add the track to the playlist.
            await _trackRepository.Upsert(newTrack);
            await _spotifyClientService.AddTrackToPlaylist(newTrack);
        }

        /// <summary>
        /// Reply when a track has been added to the playlist.
        /// </summary>
        private async Task SendReplyMessage(Message message, Track track)
        {
            var originalMessageId = message.MessageId;

            await _sendMessageService.SendTextMessageAsync(
                message,
                _successResponseService.GetSuccessResponseText(message, track),
                replyToMessageId: originalMessageId,
                replyMarkup: new InlineKeyboardMarkup(_upvoteService.CreateUpvoteButton()));
        }
    }
}
