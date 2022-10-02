using System;

namespace SpotiBot.Library.Exceptions
{
    public class PlaylistNullException : Exception
    {
        public PlaylistNullException(string playlistId)
        {
            Data[nameof(playlistId)] = playlistId;
        }
    }
}
