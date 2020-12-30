using AutoMapper;
using Sentry;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Api
{
    public class SpotifyClientService : ISpotifyClientService
    {
        private readonly IMapper _mapper;

        private const string _trackInlineBaseUri = "spotify:track:";

        public SpotifyClientService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<Track> GetTrack(ISpotifyClient spotifyClient, string trackId)
        {
            return _mapper.Map<Track>(await GetTrackFromApi(spotifyClient, trackId));
        }

        public async Task<Playlist> GetPlaylist(ISpotifyClient spotifyClient, string playlistId)
        {
            return _mapper.Map<Playlist>(await GetPlaylistFromApi(spotifyClient, playlistId));
        }

        public async Task<List<Track>> GetTracks(ISpotifyClient spotifyClient, string playlistId)
        {
            var playlist = await GetPlaylistFromApi(spotifyClient, playlistId);

            if (playlist == null)
                throw new PlaylistNullException(playlistId);

            var allTracks = await spotifyClient.PaginateAll(playlist.Tracks);

            // Tracks can be podcasts or fulltracks.
            var fullTracks = allTracks.Select(x => x.Track as FullTrack).ToList();

            var tracks = _mapper.Map<List<Track>>(fullTracks);

            foreach (var track in tracks)
                track.PlaylistId = playlistId;

            return tracks;
        }

        public async Task AddTrackToPlaylist(ISpotifyClient spotifyClient, Track track)
        {
            // Add the track to the playlist.
            await spotifyClient.Playlists.AddItems(track.PlaylistId, new PlaylistAddItemsRequest(new List<string>
            {
                $"{_trackInlineBaseUri}{track.Id}"
            }));
        }

        public async Task RemoveTrackFromPlaylist(ISpotifyClient spotifyClient, string trackId, string playlistId)
        {
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
            await spotifyClient.Playlists.RemoveItems(playlistId, removeRequest);
        }

        public async Task<bool> AddToQueue(ISpotifyClient spotifyClient, Track track)
        {
            try
            {
                await spotifyClient.Player.AddToQueue(new PlayerAddToQueueRequest($"{_trackInlineBaseUri}{track.Id}"));
                return true;
            }
            catch (APIException exception)
            {
                // Adding to the queue only works when I'm playing something in Spotify, else we get an NotFound response.
                if (exception?.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }

        private async Task<FullTrack> GetTrackFromApi(ISpotifyClient spotifyClient, string trackId)
        {
            try
            {
                return await spotifyClient.Tracks.Get(trackId);
            }
            catch (APIException exception)
            {
                SentrySdk.CaptureException(exception);
                return null;
            }
        }

        private Task<FullPlaylist> GetPlaylistFromApi(ISpotifyClient spotifyClient, string playlistId)
        {
            try
            {
                return spotifyClient.Playlists.Get(playlistId);
            }
            catch (APIException exception)
            {
                SentrySdk.CaptureException(exception);
                return null;
            }
        }
    }
}
