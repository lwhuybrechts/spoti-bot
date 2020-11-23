﻿using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Interfaces
{
    public interface ISpotifyLinkHelper
    {
        string GetMarkdownLinkToPlaylist(string text = "");
        bool HasAnySpotifyLink(string text);
        Task<string> ParseTrackId(string text);
    }
}
