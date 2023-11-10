using SpotifyAPI.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using System;

namespace SpotiBot.Library.Spotify.Api
{
    public interface ISpotifyClientService
    {
        Task<Track?> GetTrack(ISpotifyClient spotifyClient, string trackId, string playlistId, long addedByTelegramUserId, DateTimeOffset createdAt, TrackState state);
        Task<Playlist?> GetPlaylist(ISpotifyClient spotifyClient, string playlistId);
        Task<List<FullTrack>> GetTracks(ISpotifyClient spotifyClient, string playlistId);
        Task AddTrackToPlaylist(ISpotifyClient spotifyClient, string trackId, string playlistId);
        Task RemoveTrackFromPlaylist(ISpotifyClient spotifyClient, string trackId, string playlistId);
        Task<bool> AddToQueue(ISpotifyClient spotifyClient, Track track);
    }
}
