using SpotiApiModels;
using System.Collections.Generic;
using System.Linq;

namespace SpotiBot.ApiModels
{
    public class Mapper : IMapper
    {
        public Track Map(Spotify.Tracks.Track source) => new(
            source.Id,
            source.AddedByTelegramUserId,
            source.CreatedAt
        );

        public List<Track> Map(List<Spotify.Tracks.Track> source)
        {
            return source.Select(Map).ToList();
        }

        public Upvote Map(Bot.Votes.Vote source) => new(
            source.TrackId,
            (int)source.UserId,
            source.CreatedAt
        );

        public List<Upvote> Map(List<Bot.Votes.Vote> source)
        {
            return source.Select(Map).ToList();
        }

        public User Map(Bot.Users.User source) => new(
            source.Id.ToString(),
            source.FirstName
        );

        public List<User> Map(List<Bot.Users.User> source)
        {
            return source.Select(Map).ToList();
        }

        public SpotifyAccessToken Map(Spotify.Authorization.AuthorizationToken source) => new(
            source.AccessToken,
            source.TokenType,
            source.ExpiresIn,
            source.Scope,
            source.RefreshToken,
            source.CreatedAt
        );
    }
}
