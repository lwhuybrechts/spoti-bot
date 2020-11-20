using Microsoft.Azure.Cosmos.Table;

namespace Spoti_bot.Spotify.Data
{
    public class Track : TableEntity
    {
        public string Id { get; set; }
    }
}
