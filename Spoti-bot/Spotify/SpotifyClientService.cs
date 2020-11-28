using AutoMapper;
using Microsoft.Extensions.Options;
using Sentry;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Tracks;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify
{
    public class SpotifyClientService : ISpotifyClientService
    {
        private readonly IAuthorizationService _spotifyAuthorizationService;
        private readonly IMapper _mapper;
        private readonly PlaylistOptions _playlistOptions;

        private readonly Lazy<Task<ISpotifyClient>> _spotifyClient;

        private const string _trackInlineBaseUri = "spotify:track:";

        public SpotifyClientService(
            IAuthorizationService spotifyAuthorizationService,
            IMapper mapper,
            IOptions<PlaylistOptions> playlistOptions)
        {
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _mapper = mapper;
            _playlistOptions = playlistOptions.Value;

            _spotifyClient = new Lazy<Task<ISpotifyClient>>(async () =>
            {
                // This has a database dependency, so create it here instead of in startup.
                return await _spotifyAuthorizationService.CreateSpotifyClient();
            });
        }

        public async Task<bool> HasClient()
        {
            return await _spotifyClient.Value != null;
        }

        public async Task<Track> GetTrack(string trackId)
        {
            var client = await _spotifyClient.Value;

            try
            {
                var spotifyTrack = await client.Tracks.Get(trackId);
                return _mapper.Map<Track>(spotifyTrack);
            }
            catch (APIException exception)
            {
                SentrySdk.CaptureException(exception);
                return null;
            }
        }

        public async Task<FullPlaylist> GetPlaylist()
        {
            var client = await _spotifyClient.Value;

            // TODO: catch exception and return null.
            return await client.Playlists.Get(_playlistOptions.Id);
        }

        public async Task<List<Track>> GetAllTracks(Paging<PlaylistTrack<IPlayableItem>> firstPage)
        {
            var client = await _spotifyClient.Value;

            var playlistTracks = await client.PaginateAll(firstPage);
            var fullTracks = playlistTracks.Select(x => x.Track as FullTrack).ToList();

            // TODO: handle exception?

            return _mapper.Map<List<Track>>(fullTracks);
        }

        public async Task AddTrackToPlaylist(Track track)
        {
            var client = await _spotifyClient.Value;

            // Add the track to the playlist.
            await client.Playlists.AddItems(_playlistOptions.Id, new PlaylistAddItemsRequest(new List<string>
            {
                $"{_trackInlineBaseUri}{track.Id}"
            }));
        }

        public async Task RemoveTrackFromPlaylist(string trackId)
        {
            var client = await _spotifyClient.Value;

            var removeRequest = new PlaylistRemoveItemsRequest
            {
                Tracks = new List<PlaylistRemoveItemsRequest.Item>
                {
                    new PlaylistRemoveItemsRequest.Item
                    {
                        Uri = $"{_trackInlineBaseUri}{trackId}"
                    }
                }
            };

            // Remove the track from the playlist.
            await client.Playlists.RemoveItems(_playlistOptions.Id, removeRequest);
        }

        public async Task AddToQueue(Track track)
        {
            var client = await _spotifyClient.Value;

            try
            {
                await client.Player.AddToQueue(new PlayerAddToQueueRequest($"{_trackInlineBaseUri}{track.Id}"));
                return;
            }
            catch (APIException exception)
            {
                // Adding to the queue only works when I'm playing something in Spotify, else we get an NotFound response.
                if (exception?.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return;

                throw;
            }
        }
    }
}
