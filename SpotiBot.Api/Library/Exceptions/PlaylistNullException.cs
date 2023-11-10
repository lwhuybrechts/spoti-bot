using System;

namespace SpotiBot.Api.Library.Exceptions
{
    public class PlaylistNullException : Exception
    {
        public PlaylistNullException(string playlistId)
        {
            Data[nameof(playlistId)] = playlistId;
        }
    }
}
