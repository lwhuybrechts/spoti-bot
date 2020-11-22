using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Spotify.Data.Tracks
{
    public class Track : TableEntity
    {
        public string Id { get; set; }
        public string FirstArtistName { get; set; }
        public string AlbumName { get; set; }
    }
}
