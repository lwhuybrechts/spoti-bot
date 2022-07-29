using System.ComponentModel;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    /// <summary>
    /// The list with supported commands and the text that triggers them.
    /// </summary>
    public enum Command
    {
        [Description("/test")]
        Test,

        [Description("/help")]
        [RequiresChat, RequiresPlaylist]
        Help,

        [Description("/start")]
        [RequiresNoChat]
        Start,
                
        [Description("/login")]
        [RequiresPrivateChat]
        GetLoginLink,
        
        // Can be used while testing, when the playlist was edited in Spotify.
        [Description("/reset")]
        [RequiresChat, RequiresChatAdmin, RequiresPlaylist]
        ResetPlaylistStorage,
        
        [Description("/setplaylist")]
        [RequiresChat, RequiresChatAdmin, RequiresNoPlaylist, RequiresQuery]
        SetPlaylist,

        [Description("/webapp")]
        [RequiresPrivateChat]
        WebApp
    }
}
