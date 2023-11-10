using System.Collections.Generic;
using System.Linq;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using SpotifyAPI.Web;

namespace SpotiBot.Library.ApiModels
{
    public class Mapper : IMapper
    {
        public Track Map(BusinessModels.Spotify.Track source) => new(
            source.Id,
            source.PlaylistId,
            source.AddedByTelegramUserId,
            source.CreatedAt
        );

        public List<Track> Map(List<BusinessModels.Spotify.Track> source)
        {
            return source.Select(Map).ToList();
        }

        public List<BusinessModels.Spotify.Track> Map(List<Track> tracks, List<FullTrack> apiTracks)
        {
            var tracksList = new List<BusinessModels.Spotify.Track>();
            foreach (var track in tracks)
            {
                var apiTrack = apiTracks.FirstOrDefault(x => x.Id == track.Id);

                // Skip tracks that cannot be found in the spotify api.
                if (apiTrack != null)
                    tracksList.Add(Map(track, apiTrack));
            }

            return tracksList;
        }

        public BusinessModels.Spotify.Track Map(Track track, FullTrack fullTrack) => new(
            track.Id,
            track.PlaylistId,
            fullTrack.Name,
            fullTrack.Artists.First().Name,
            fullTrack.Album.Name,
            track.AddedByTelegramUserId,
            track.AddedAt,
            TrackState.AddedToPlaylist
        );

        public Upvote Map(Vote source) => new(
            source.TrackId,
            (int)source.UserId,
            source.CreatedAt
        );

        public List<Upvote> Map(List<Vote> source)
        {
            return source.Select(Map).ToList();
        }

        public User Map(BusinessModels.Bot.User source) => new(
            source.Id.ToString(),
            source.FirstName
        );

        public List<User> Map(List<BusinessModels.Bot.User> source)
        {
            return source.Select(Map).ToList();
        }

        public SpotifyAccessToken Map(AuthorizationToken source) => new(
            source.AccessToken,
            source.TokenType,
            source.ExpiresIn,
            source.Scope,
            source.RefreshToken,
            source.CreatedAt
        );

        public AuthorizationToken Map(SpotifyAccessToken source, long userId) => new(
            userId,
            source.AccessToken,
            source.TokenType,
            source.ExpiresIn,
            source.Scope,
            source.RefreshToken,
            source.CreatedAt
        );
    }
}
