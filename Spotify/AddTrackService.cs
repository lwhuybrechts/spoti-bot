using Spoti_bot.Bot;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Data.Tracks;
using Spoti_bot.Spotify.Interfaces;
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

        public async Task<bool> TryAddTrackToPlaylist(Message message)
        {
            // Parse the trackId from the message.
            var newTrackId = await ParseTrackIdFromMessage(message);

            // Check if the track already exists in the playlist.
            if (await _trackRepository.Get(newTrackId) != null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist("playlist")}!");
                
                // The track is already in the playlist, so return true.
                return true;
            }

            // We can't continue if we can't use the spotify api.
            if (!await _spotifyClientService.HasClient())
            {
                await _sendMessageService.SendTextMessageAsync(message, "Spoti-bot is not authorized to add this track to Spotify.");
                return false;
            }

            // Get the track from the spotify api.
            var newTrack = await _spotifyClientService.GetTrack(newTrackId);
            if (newTrack == null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"Track not found in Spotify api :(");
                return false;
            }
            
            newTrack.AddedByTelegramUserId = message.From.Id;

            // Add the track to the playlist.
            await _trackRepository.Upsert(newTrack);
            await _spotifyClientService.AddTrackToPlaylist(newTrack);

            // Reply that the message has been added successfully.
            await SendReplyMessage(message, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(newTrack);

            return true;
        }

        /// <summary>
        /// Reply when a track has been added to our playlist.
        /// </summary>
        private async Task SendReplyMessage(Message message, Track track)
        {
            var originalMessageId = message.MessageId;
            var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(UpvoteHelper.ButtonText));
            var successText = _successResponseService.GetSuccessResponseText(message, track);
            
            await _sendMessageService.SendTextMessageAsync(message, successText, replyToMessageId: originalMessageId, replyMarkup: keyboard);
        }

        // TODO: move to SpotifyLinkHelper, what to do with httpClient dependency?
        private async Task<string> ParseTrackIdFromMessage(Message message)
        {
            // Check for a "classic" spotify url.
            if (_spotifyLinkHelper.HasTrackIdLink(message.Text))
                return _spotifyLinkHelper.ParseTrackId(message.Text);

            // Check for a "linkto" spotify url.
            if (_spotifyLinkHelper.HasToSpotifyLink(message.Text))
            {
                var linkToUri = _spotifyLinkHelper.ParseToSpotifyLink(message.Text);
                
                // Get the trackUri from the linkToUri.
                var trackUri = await RequestTrackUri(linkToUri);

                if (_spotifyLinkHelper.HasTrackIdLink(trackUri))
                    return _spotifyLinkHelper.ParseTrackId(trackUri);
            }

            // This should not happen, so log it to Sentry.
            throw new TrackIdNullException();
        }

        private async Task<string> RequestTrackUri(string linkToUri)
        {
            // TODO: reuse httpclient from startup.
            var client = new System.Net.Http.HttpClient();
            
            // TODO: currently we get a badrequest, but the trackUri we're looking for is in it's request.
            // This feels pretty hacky, not sure if it will keep working in the future.
            var response = await client.GetAsync(linkToUri);
            var trackUri = response?.RequestMessage?.RequestUri?.AbsoluteUri ?? "";

            return trackUri;
        }
    }
}
