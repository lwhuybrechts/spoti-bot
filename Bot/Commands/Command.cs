using System.ComponentModel;

namespace Spoti_bot.Bot.Commands
{
    /// <summary>
    /// The list with supported commands and the text that triggers them.
    /// </summary>
    public enum Command
    {
        [Description("/help")]
        Help,
        [Description("/test")]
        Test,
        // Fetching an accesstoken is only needed once, since it's saved in storage and we can keep refreshing it.
        // Therefore we don't expose this command to the end users (yet).
        [Description("/geheimcommando")]
        GetLoginLink,
        // Can be used while testing, when the playlist was edited in Spotify.
        [Description("/reset")]
        ResetPlaylistStorage
    }
}
