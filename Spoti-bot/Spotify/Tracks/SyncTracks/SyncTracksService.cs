using Spoti_bot.Library.Exceptions;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncTracks
{
    public class SyncTracksService : ISyncTracksService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly ISpotifyClientService _spotifyClientService;

        public SyncTracksService(ITrackRepository trackRepository, ISpotifyClientService spotifyClientService)
        {
            _trackRepository = trackRepository;
            _spotifyClientService = spotifyClientService;
        }

        /// <summary>
        /// Get all playlist tracks from the Spotify api and save them in our storage.
        /// This can be helpful if the playlist was edited in Spotify.
        /// </summary>
        public async Task SyncTracks()
        {
            var playlist = await GetPlaylistFromSpotify();
            if (playlist == null)
                throw new PlaylistNullException();

            var tracksFromSpotify = await GetTracksFromSpotify(playlist);

            // TODO: this removes any properties that are not in the spotify tracks :(
            if (tracksFromSpotify.Any())
                await SaveTracksToStorage(tracksFromSpotify);

            // Get all tracks from storage.
            var tracks = await GetTracksFromStorage();

            // Remove any tracks from storage that are not in the playlist.
            var tracksToDelete = FilterTracksToDeleteFromStorage(tracksFromSpotify, tracks);
            if (tracksToDelete.Any())
                await DeleteTracks(tracksToDelete);
        }

        /// <summary>
        /// Get all tracks that are in the storage but not in the spotify playlist.
        /// </summary>
        private List<Track> FilterTracksToDeleteFromStorage(List<Track> tracksFromSpotify, List<Track> tracksFromStorage)
        {
            return tracksFromStorage.Where(track =>
                !tracksFromSpotify.Select(x => x.Id).Contains(track.Id)
            ).ToList();
        }

        private async Task<FullPlaylist> GetPlaylistFromSpotify()
        {
            return await _spotifyClientService.GetPlaylist();
        }

        private async Task<List<Track>> GetTracksFromSpotify(FullPlaylist playlist)
        {
            return await _spotifyClientService.GetAllTracks(playlist.Tracks);
        }

        private async Task SaveTracksToStorage(List<Track> tracks)
        {
            await _trackRepository.Upsert(tracks);
        }

        private async Task<List<Track>> GetTracksFromStorage()
        {
            return await _trackRepository.GetAll();
        }

        private async Task DeleteTracks(List<Track> tracksToDelete)
        {
            await _trackRepository.Delete(tracksToDelete);
        }
    }
}
