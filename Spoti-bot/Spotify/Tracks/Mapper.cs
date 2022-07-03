using Spoti_bot.Spotify.Tracks.SyncHistory;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;

namespace Spoti_bot.Spotify.Tracks
{
    public class Mapper : IMapper
    {
        public Track Map(FullTrack source) => new()
        {
            Id = source.Id,
            Name = source.Name,
            FirstArtistName = source.Artists.FirstOrDefault().Name,
            AlbumName = source.Album.Name
        };

        public List<Track> Map(List<FullTrack> source)
        {
            return source.Select(Map).ToList();
        }

        public Track Map(string source) => new()
        {
            Id = source
        };

        public Track Map(TrackAdded source) => new()
        {
            Id = source.TrackId,
            AddedByTelegramUserId = source.FromId,
            CreatedAt = source.CreatedAt
        };

        public List<Track> Map(List<TrackAdded> source)
        {
            return source.Select(Map).ToList();
        }
    }
}
