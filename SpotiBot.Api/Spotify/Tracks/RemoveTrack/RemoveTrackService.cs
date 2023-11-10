using SpotiBot.Api.Bot;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using SpotiBot.Data.Services;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using SpotiBot.Library.Spotify.Api;
using SpotifyAPI.Web;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.RemoveTrack
{
    public class RemoveTrackService : IRemoveTrackService
    {
        private readonly ITrackService _trackService;
        private readonly IAuthorizationTokenService _authorizationTokenService;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ISendMessageService _sendMessageService;

        public RemoveTrackService(
            ITrackService trackService,
            IAuthorizationTokenService authorizationTokenService,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            ISendMessageService sendMessageService)
        {
            _trackService = trackService;
            _authorizationTokenService = authorizationTokenService;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _sendMessageService = sendMessageService;
        }

        public async Task<BotResponseCode> TryRemoveTrackFromPlaylist(UpdateDto updateDto)
        {
            if (updateDto.Chat == null ||
                updateDto.Track == null)
                return BotResponseCode.CommandRequirementNotFulfilled;

            var token = await _authorizationTokenService.Get(updateDto.Chat.AdminUserId);

            // We can't continue if we can't use the spotify api.
            if (token == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, "Spoti-bot is not authorized to remove this track from the Spotify playlist.");
                return BotResponseCode.CommandRequirementNotFulfilled;
            }

            var spotifyClient = _spotifyClientFactory.Create(token);

            await RemoveTrack(spotifyClient, updateDto.Track);

            return BotResponseCode.TrackRemovedFromPlaylist;
        }

        private async Task RemoveTrack(ISpotifyClient spotifyClient, Track track)
        {
            // Mark the track as removed and save it.
            track.State = TrackState.RemovedByDownvotes;

            await _trackService.Upsert(track);

            // Remove the track from the spotify playlist.
            await _spotifyClientService.RemoveTrackFromPlaylist(spotifyClient, track.Id, track.PlaylistId);
        }
    }
}
