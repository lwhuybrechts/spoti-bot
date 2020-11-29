﻿using Spoti_bot.Spotify.Tracks;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify
{
    public interface ISpotifyClientService
    {
        Task<bool> HasClient();
        Task<Track> GetTrack(string trackId);
        Task<FullPlaylist> GetPlaylist();
        Task<List<Track>> GetAllTracks(Paging<PlaylistTrack<IPlayableItem>> firstPage);
        Task AddTrackToPlaylist(Track track);
        Task RemoveTrackFromPlaylist(string trackId);
        Task AddToQueue(Track track);
    }
}