using SpotifyAPI.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;

namespace SpotiBot.Library.Spotify.Api
{
    public class SpotifyClientService : ISpotifyClientService
    {
        private readonly Tracks.IMapper _tracksMapper;
        private readonly Playlists.IMapper _playlistsMapper;

        private const string _trackInlineBaseUri = "spotify:track:";

        public SpotifyClientService(Tracks.IMapper tracksMapper, Playlists.IMapper playlistsMapper)
        {
            _tracksMapper = tracksMapper;
            _playlistsMapper = playlistsMapper;
        }

        public async Task<Track?> GetTrack(ISpotifyClient spotifyClient, string trackId, string playlistId, long addedByTelegramUserId, DateTimeOffset createdAt, TrackState state)
        {
            return _tracksMapper.Map(await GetTrackFromApi(spotifyClient, trackId), playlistId, addedByTelegramUserId, createdAt, state);
        }

        public async Task<Playlist?> GetPlaylist(ISpotifyClient spotifyClient, string playlistId)
        {
            return _playlistsMapper.Map(await GetPlaylistFromApi(spotifyClient, playlistId));
        }

        public async Task<List<FullTrack>> GetTracks(ISpotifyClient spotifyClient, string playlistId)
        {
            var playlist = await GetPlaylistFromApi(spotifyClient, playlistId);

            if (playlist == null || playlist.Tracks == null)
                throw new Exception(playlistId); //PlaylistNullException(playlistId);

            var allTracks = await spotifyClient.PaginateAll(playlist.Tracks);

            // Tracks can be podcasts or fulltracks.
            List<FullTrack> fullTracks = allTracks.Select(x => x.Track as FullTrack).ToList();

            return fullTracks;
        }

        public async Task AddTrackToPlaylist(ISpotifyClient spotifyClient, string trackId, string playlistId)
        {
            // Add the track to the playlist.
            await spotifyClient.Playlists.AddItems(playlistId, new PlaylistAddItemsRequest(new List<string>
            {
                $"{_trackInlineBaseUri}{trackId}"
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
                // Adding to the queue only works when the user is playing something in Spotify, else we get a NotFound response.
                if (exception?.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }

        private static Task<FullTrack> GetTrackFromApi(ISpotifyClient spotifyClient, string trackId)
        {
            return spotifyClient.Tracks.Get(trackId);
        }

        private static Task<FullPlaylist> GetPlaylistFromApi(ISpotifyClient spotifyClient, string playlistId)
        {
            return spotifyClient.Playlists.Get(playlistId);
        }
    }
}
