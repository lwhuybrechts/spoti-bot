using Spoti_bot.Bot;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using Spoti_bot.Spotify.Api;
using SpotifyAPI.Web;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.RemoveTrack
{
    public class RemoveTrackService : IRemoveTrackService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly ISendMessageService _sendMessageService;

        public RemoveTrackService(
            ITrackRepository trackRepository,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            ISendMessageService sendMessageService)
        {
            _trackRepository = trackRepository;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _sendMessageService = sendMessageService;
        }

        public async Task<BotResponseCode> TryRemoveTrackFromPlaylist(UpdateDto updateDto)
        {
            if (updateDto.Chat == null ||
                updateDto.Track == null)
                return BotResponseCode.NoAction;

            var spotifyClient = await _spotifyClientFactory.Create(updateDto.Chat.AdminUserId);

            // We can't continue if we can't use the spotify api.
            if (spotifyClient == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, "Spoti-bot is not authorized to remove this track from the Spotify playlist.");
                return BotResponseCode.NoAction;
            }

            if (updateDto.Track == null)
            {
                await _sendMessageService.SendTextMessage(updateDto.Chat.Id, "Track not found in storage :(");
                return BotResponseCode.NoAction;
            }

            await RemoveTrack(spotifyClient, updateDto.Track);

            return BotResponseCode.TrackRemovedFromPlaylist;
        }

        private async Task RemoveTrack(ISpotifyClient spotifyClient, Track track)
        {
            await _trackRepository.Delete(track);
            // Remove the track from the spotify playlist.
            await _spotifyClientService.RemoveTrackFromPlaylist(spotifyClient, track.Id, track.PlaylistId);
        }
    }
}
