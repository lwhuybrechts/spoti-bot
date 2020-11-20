using AutoMapper;
using Microsoft.Extensions.Options;
using Sentry;
using Spoti_bot.Bot;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Data;
using SpotifyAPI.Web;
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
        private readonly ISpotifyAuthorizationService _spotifyAuthorizationService;
        private readonly ISpotifyLinkHelper _spotifyTextHelper;
        private readonly ITrackRepository _trackRepository;
        private readonly IMapper _mapper;
        private readonly PlaylistOptions _playlistOptions;

        public SpotifyAddTrackService(ISendMessageService sendMessageService, ISpotifyAuthorizationService spotifyAuthorizationService, ISpotifyLinkHelper spotifyTextHelper, ITrackRepository trackRepository, IMapper mapper, IOptions<PlaylistOptions> playlistOptions)
        {
            _sendMessageService = sendMessageService;
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _spotifyTextHelper = spotifyTextHelper;
            _trackRepository = trackRepository;
            _mapper = mapper;
            _playlistOptions = playlistOptions.Value;
        }

        // TODO: refactor some more.
        public async Task<bool> TryAddTrackToPlaylist(Message message)
        {
            // TODO: inject in Startup.
            var spotifyClient = await _spotifyAuthorizationService.CreateSpotifyClient();

            // We can't continue if we aren't logged in to the spotify api.
            if (spotifyClient == null)
            {
                await SendMessageToChat(message, "Spoti-bot is not authorized to add this track to Spotify.");
                return false;
            }

            // Parse the track from the message.
            var track = await ParseTrackFromMessage(message);

            if (track == null)
            {
                // If the message does not contain a track, we're done.
                return true;
            }

            // Get the track from the spotify api.
            var spotifyTrack = await GetTrackFromSpotify(track, message, spotifyClient);

            if (spotifyTrack == null)
            {
                await SendMessageToChat(message, $"Track not found in Spotify api :(");
                return false;
            }

            // Get all tracks from storage.
            // TODO: use Playlist model here.
            FullPlaylist spotifyPlaylist = null;
            var tracks = await GetTracksFromStorage();

            // If the storage is empty, fetch the tracks from spotify api.
            if (!tracks.Any())
            {
                spotifyPlaylist = await GetPlaylistFromSpotify(spotifyClient);
                if (spotifyPlaylist == null)
                {
                    await SendMessageToChat(message, $"Playlist {GetPlaylistLink()} not found.");
                    return false;
                }

                tracks = await FetchAllTracksFromSpotify(spotifyClient, spotifyPlaylist.Tracks);
                await SaveTracksToStorage(tracks);
            }

            // Check if the track already exists in the playlist.
            if (tracks.Select(x => x.Id).Contains(track.Id))
            {
                await SendMessageToChat(message, $"This track is already added to the {GetPlaylistLink("playlist")}!");
                return true;
            }

            // Only fetch the playlist if we didn't already.
            if (spotifyPlaylist == null)
                spotifyPlaylist = await GetPlaylistFromSpotify(spotifyClient);

            if (spotifyPlaylist == null)
            {
                await SendMessageToChat(message, $"Playlist {GetPlaylistLink()} not found.");
                return false;
            }

            // If the storage is not up to date, fetch the tracks from spotify api.
            if (spotifyPlaylist.Tracks.Total.GetValueOrDefault() != tracks.Count)
            {
                // TODO: only save/sync differences.

                // Add the tracks to the storage.
                tracks = await FetchAllTracksFromSpotify(spotifyClient, spotifyPlaylist.Tracks);
                tracks.Add(track);
                await SaveTracksToStorage(tracks);
            }
            else
            {
                // Add the track to the storage.
                await SaveTrackToStorage(track);
            }

            // Add the track to our playlist.
            await AddTrackToSpotifyPlaylist(spotifyClient, track);

            var originalMessageId = message.MessageId;
            var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(UpvoteHelper.ButtonText));
            await SendMessageToChat(message, GetSuccessResponseText(message, spotifyTrack), replyToMessageId: originalMessageId, replyMarkup: keyboard);




            // TODO: testing....
            var album = await spotifyClient.Albums.Get(spotifyTrack.Album.Id);

            if (album?.Genres?.Count > 0)
            {
                var sentryEvent = new SentryEvent
                {
                    Message = $"Genres found."
                };
                sentryEvent.SetTag("Genres", string.Join(", ", album.Genres));

                SentrySdk.CaptureEvent(sentryEvent);
            }

            // Add the track to my queue.
            try
            {
                await spotifyClient.Player.AddToQueue(new PlayerAddToQueueRequest($"{SpotifyLinkHelper.TrackInlineBaseUri}{track.Id}"));
            }
            catch (APIException exception)
            {
                // Adding to the queue only works when I'm playing something in Spotify, else we get an NotFound response.
                if (exception?.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return true;

                throw;
            }

            return true;
        }

        // TODO: move to SendMessageService as overload?
        private async Task SendMessageToChat(Message message, string textMessage, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            await _sendMessageService.SendTextMessageAsync(message.Chat.Id, textMessage, replyToMessageId: replyToMessageId, replyMarkup: replyMarkup);
        }

        // TODO: move to their own services.
        private string GetSuccessResponseText(Message message, FullTrack track)
        {
            var successMessage = $"Track added to the {GetPlaylistLink("playlist")}!";

            var random = new Random();
            if (!ShouldAddAwesomeResponse(random))
                return successMessage;

            var firstName = message?.From?.FirstName;

            if (string.IsNullOrEmpty(firstName))
                return successMessage;

            return GetRandomAwesomeResponse(random, successMessage, firstName, track);
        }

        private string GetRandomAwesomeResponse(Random random, string successMessage, string firstName, FullTrack track)
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

            var firstArtistName = track?.Artists?.FirstOrDefault()?.Name;
            if (!string.IsNullOrEmpty(firstArtistName))
                responses.Add($"Always love me some {firstArtistName}!");

            var albumName = track?.Album?.Name;
            if (!string.IsNullOrEmpty(albumName))
                responses.Add($"Also check out it's album, {albumName}.");

            return $"{successMessage} {responses[random.Next(0, responses.Count)]}";
        }

        /// <summary>
        /// Sometimes we want to add an awesome response to the successText, but not always since that might get lame.
        /// </summary>
        private static bool ShouldAddAwesomeResponse(Random random)
        {
            // 1 in 5 chance we add an awesome response.
            return random.Next(0, 5) == 0;
        }

        private async Task<FullTrack> GetTrackFromSpotify(Track trackId, Message message, ISpotifyClient spotifyClient)
        {
            try
            {
                return await spotifyClient.Tracks.Get(trackId.Id);
            }
            catch (APIException exception)
            {
                if (exception.Message == "invalid id")
                    await _sendMessageService.SendTextMessageAsync(message.Chat.Id, $"Track not found in Spotify api :(");

                SentrySdk.CaptureException(exception);
                return null;
            }
        }

        private async Task<FullPlaylist> GetPlaylistFromSpotify(ISpotifyClient spotifyClient)
        {
            // TODO: catch exception and return null.
            return await spotifyClient.Playlists.Get(_playlistOptions.Id);
        }

        private async Task<List<Track>> FetchAllTracksFromSpotify(ISpotifyClient spotifyClient, Paging<PlaylistTrack<IPlayableItem>> firstPage)
        {
            var playlistTracks = await spotifyClient.PaginateAll(firstPage);
            var fullTracks = playlistTracks.Select(x => x.Track as FullTrack).ToList();

            // TODO: handle exception?

            return _mapper.Map<List<Track>>(fullTracks);
        }

        private async Task AddTrackToSpotifyPlaylist(ISpotifyClient spotifyClient, Track track)
        {
            // Add the track to our playlist.
            await spotifyClient.Playlists.AddItems(_playlistOptions.Id, new PlaylistAddItemsRequest(new List<string> { $"{SpotifyLinkHelper.TrackInlineBaseUri}{track.Id}" }));
        }

        private async Task<Track> ParseTrackFromMessage(Message message)
        {
            // First, check for a "classic" spotify url.
            if (_spotifyTextHelper.HasTrackIdLink(message.Text))
                return _spotifyTextHelper.ParseTrackId(message.Text);

            // If there is no "new" linkto spotify url, we can't find the trackId.
            if (!_spotifyTextHelper.HasToSpotifyLink(message.Text))
                return null;

            var linkToUri = _spotifyTextHelper.ParseToSpotifyLink(message.Text);
            var trackUri = await RequestTrackUri(linkToUri);

            if (_spotifyTextHelper.HasTrackIdLink(trackUri))
                return _spotifyTextHelper.ParseTrackId(trackUri);

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

        private string GetPlaylistLink(string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = _playlistOptions.Name;

            return $"[{text}]({SpotifyLinkHelper.PlaylistBaseUri}{_playlistOptions.Id})";
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
    }
}
