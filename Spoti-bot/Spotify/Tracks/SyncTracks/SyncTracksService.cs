﻿using SpotiBot.Spotify.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Tracks.SyncTracks
{
    public class SyncTracksService : ISyncTracksService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;

        public SyncTracksService(ITrackRepository trackRepository, ISpotifyClientFactory spotifyClientFactory, ISpotifyClientService spotifyClientService)
        {
            _trackRepository = trackRepository;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
        }

        /// <summary>
        /// Get all playlist tracks from the Spotify api and save them in our storage.
        /// This can be helpful if the playlist was edited in Spotify.
        /// </summary>
        /// <param name="chat">The chat of which the playlist should be synced.</param>
        /// <param name="shouldUpdateExistingTracks">If true, overwrites track info from spotify. This is only need when Spotify changes track info after the tracks were added to storage.</param>
        public async Task SyncTracks(Bot.Chats.Chat chat, bool shouldUpdateExistingTracks = false)
        {
            var spotifyClient = await _spotifyClientFactory.Create(chat.AdminUserId);

            if (spotifyClient == null)
                return;

            var tracksFromSpotify = await _spotifyClientService.GetTracks(spotifyClient, chat.PlaylistId);
            var tracksFromStorage = await GetTracks(chat.PlaylistId);

            // Update track info of all tracks in both the storage and spotify playlist.
            var tracksToUpdate = FilterTracksToUpdate(tracksFromSpotify, tracksFromStorage, shouldUpdateExistingTracks);
            if (tracksToUpdate.Any())
                await SaveTracks(tracksToUpdate);

            // Add any missing tracks to storage that are in the spotify playlist.
            var tracksToSave = FilterTracksToSave(tracksFromSpotify, tracksFromStorage);
            if (tracksToSave.Any())
                await SaveTracks(tracksFromSpotify);

            // Remove any tracks from storage that are not in the spotify playlist.
            var tracksToDelete = FilterTracksToDelete(tracksFromSpotify, tracksFromStorage);
            if (tracksToDelete.Any())
                await DeleteTracks(tracksToDelete);
        }

        /// <summary>
        /// Get all tracks that are in both the spotify playlist and in the storage, and update some properties from spotify.
        /// </summary>
        private static List<Track> FilterTracksToUpdate(List<Track> tracksFromSpotify, List<Track> tracksFromStorage, bool shouldUpdateExistingTracks)
        {
            if (!shouldUpdateExistingTracks)
                return new List<Track>();

            var tracksToUpdate = tracksFromStorage.Where(track =>
                tracksFromSpotify.Any(x => x.Id == track.Id)
            ).ToList();

            foreach (var trackToUpdate in tracksToUpdate)
            {
                var trackFromSpotify = tracksFromSpotify.First(x => x.Id == trackToUpdate.Id);

                // Overwrite some properties.
                trackToUpdate.Name = trackFromSpotify.Name;
                trackToUpdate.AlbumName = trackFromSpotify.AlbumName;
                trackToUpdate.FirstArtistName = trackFromSpotify.FirstArtistName;
            }

            return tracksToUpdate;
        }

        /// <summary>
        /// Get all tracks that are in the spotify playlist but not in the storage.
        /// </summary>
        private static List<Track> FilterTracksToSave(List<Track> tracksFromSpotify, List<Track> tracksFromStorage)
        {
            return tracksFromSpotify.Where(track =>
                !tracksFromStorage.Any(x => x.Id == track.Id)
            ).ToList();
        }

        /// <summary>
        /// Get all tracks that are in the storage but not in the spotify playlist.
        /// </summary>
        private static List<Track> FilterTracksToDelete(List<Track> tracksFromSpotify, List<Track> tracksFromStorage)
        {
            return tracksFromStorage.Where(track =>
                !tracksFromSpotify.Any(x => x.Id == track.Id) &&
                // Don't delete tracks from storage that are marked as removed.
                track.State != TrackState.RemovedByDownvotes
            ).ToList();
        }

        private async Task SaveTracks(List<Track> tracks)
        {
            await _trackRepository.Upsert(tracks);
        }

        private async Task<List<Track>> GetTracks(string playlistId)
        {
            return await _trackRepository.GetAllByPartitionKey(playlistId);
        }

        private async Task DeleteTracks(List<Track> tracksToDelete)
        {
            await _trackRepository.Delete(tracksToDelete);
        }
    }
}
