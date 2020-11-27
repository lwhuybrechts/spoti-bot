using Spoti_bot.Bot;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.Spotify.Data.Tracks;
using Spoti_bot.Spotify.Interfaces;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Spotify
{
    public class AddTrackService : IAddTrackService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISuccessResponseService _successResponseService;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;

        public AddTrackService(
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyTextHelper,
            ISuccessResponseService successResponseService,
            ISpotifyClientService spotifyClientService,
            ITrackRepository trackRepository)
        {
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyTextHelper;
            _successResponseService = successResponseService;
            _spotifyClientService = spotifyClientService;
            _trackRepository = trackRepository;
        }

        public async Task<BotResponseCode> TryAddTrackToPlaylist(Message message)
        {
            // Parse the trackId from the message.
            var newTrackId = await _spotifyLinkHelper.ParseTrackId(message.Text);

            // Check if the track already exists in the playlist.
            if (await _trackRepository.Get(newTrackId) != null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist("playlist")}!");

                // The track is already in the playlist, so return true.
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
            var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(UpvoteTextHelper.ButtonText));
            var successText = _successResponseService.GetSuccessResponseText(message, track);
            
            await _sendMessageService.SendTextMessageAsync(message, successText, replyToMessageId: originalMessageId, replyMarkup: keyboard);
        }
    }
}
