using System;

namespace Spoti_bot.Library.Exceptions
{
    public class PlaylistNullException : Exception
    {
        public PlaylistNullException(string playlistId)
        {
            Data[nameof(playlistId)] = playlistId;
        }
    }
}
