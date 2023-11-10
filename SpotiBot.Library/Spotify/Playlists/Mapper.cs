using SpotiBot.Library.BusinessModels.Spotify;
using SpotifyAPI.Web;

namespace SpotiBot.Library.Spotify.Playlists
{
    public class Mapper : IMapper
    {
        //public Playlist Map(SpotiLibrary.Spotify.Playlists.Playlist source) => new()
        //{
        //    Id = source.Id,
        //    Name = source.Name
        //};

        //public SpotiLibrary.Spotify.Playlists.Playlist Map(Playlist source) => new(
        //    source.Id,
        //    source.Name
        //);

        public Playlist? Map(FullPlaylist source)
        {
            if (string.IsNullOrEmpty(source.Id) ||
                string.IsNullOrEmpty(source.Name))
                return null;

            return new(
                source.Id,
                source.Name
            );
        }
    }
}
