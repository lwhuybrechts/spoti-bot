using AutoMapper;
using Spoti_bot.Bot;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Data.Tracks;
using Spoti_bot.Spotify.Data.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Spotify
{
    public class SpotifyAddTrackService : ISpotifyAddTrackService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly ISuccessResponseService _successResponseService;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ITrackRepository _trackRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public SpotifyAddTrackService(
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyTextHelper,
            ISuccessResponseService successResponseService,
            ISpotifyClientService spotifyClientService,
            ITrackRepository trackRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyTextHelper;
            _successResponseService = successResponseService;
            _spotifyClientService = spotifyClientService;
            _trackRepository = trackRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<bool> TryAddTrackToPlaylist(Message message)
        {
            // We can't continue if we can't use the spotify api.
            if (!await _spotifyClientService.HasClient())
            {
                await _sendMessageService.SendTextMessageAsync(message, "Spoti-bot is not authorized to add this track to Spotify.");
                return false;
            }

            // Parse the trackId from the message.
            var newTrackId = await ParseTrackIdFromMessage(message);
            if (newTrackId == null)
            {
                // This should not happen, so log it to Sentry.
                throw new TrackIdNullException();
            }

            // Get the track from the spotify api.
            var newTrack = await _spotifyClientService.GetTrack(newTrackId, message);
            if (newTrack == null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"Track not found in Spotify api :(");
                return false;
            }

            // Get the playlist from the spotify api.
            var playlist = await _spotifyClientService.GetPlaylist();
            if (playlist == null)
            {
                await _sendMessageService.SendTextMessageAsync(message, $"Playlist {_spotifyLinkHelper.GetMarkdownLinkToPlaylist()} not found.");
                return false;
            }

            // Get all tracks from storage.
            var tracks = await GetTracksFromStorage();

            // If the storage is empty, fetch the tracks from spotify api.
            if (!tracks.Any())
            {
                tracks = await _spotifyClientService.GetAllTracks(playlist.Tracks);
                await SaveTracksToStorage(tracks);
            }

            // Check if the track already exists in the playlist.
            if (DoesTrackExist(newTrack, tracks))
            {
                await _sendMessageService.SendTextMessageAsync(message, $"This track is already added to the {_spotifyLinkHelper.GetMarkdownLinkToPlaylist("playlist")}!");
                return true;
            }

            // TODO: move to a different service?
            // Save the user to the storage.
            var user = _mapper.Map<Data.User.User>(message.From);
            await _userRepository.Upsert(user);

            // If the storage is not up to date with the spotify playlist, sync it.
            if (IsTrackCountOutOfSync(playlist, tracks))
            {
                // Add all tracks to the storage.
                tracks = await _spotifyClientService.GetAllTracks(playlist.Tracks);
                tracks.Add(newTrack);
                await SaveTracksToStorage(tracks);
            }
            else
            {
                // Add the new track to the storage.
                await SaveTrackToStorage(newTrack);
            }

            // Add the track to our playlist.
            await _spotifyClientService.AddTrackToPlaylist(newTrack);

            // Reply that the message has been added successfully.
            await SendReplyMessage(message, newTrack);

            // Add the track to my queue.
            await _spotifyClientService.AddToQueue(newTrack);

            return true;
        }

        /// <summary>
        /// Check if the amount of tracks in the spotify playlist is the same as in a list of tracks.
        /// </summary>
        private static bool IsTrackCountOutOfSync(SpotifyAPI.Web.FullPlaylist playlist, List<Track> tracks)
        {
            return playlist.Tracks.Total.GetValueOrDefault() != tracks.Count;
        }

        /// <summary>
        /// Check if the track already exists in our list or tracks.
        /// </summary>
        private static bool DoesTrackExist(Track newTrack, List<Track> tracks)
        {
            return tracks.Select(x => x.Id).Contains(newTrack.Id);
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

        private async Task<List<Track>> GetTracksFromStorage()
        {
            return await _trackRepository.GetAll();
        }

        private async Task SaveTracksToStorage(List<Track> tracks)
        {
            await _trackRepository.Upsert(tracks);
        }

        private async Task SaveTrackToStorage(Track track)
        {
            await _trackRepository.Upsert(track);
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

            return null;
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
