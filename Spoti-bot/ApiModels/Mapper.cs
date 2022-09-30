using System;
using System.Collections.Generic;
using System.Linq;

namespace Spoti_bot.ApiModels
{
    public class Mapper : IMapper
    {
        public Track Map(Spotify.Tracks.Track source) => new()
        {
            Id = source.Id,
            AddedByTelegramUserId = source.AddedByTelegramUserId,
            AddedAt = source.CreatedAt
        };

        public List<Track> Map(List<Spotify.Tracks.Track> source)
        {
            return source.Select(Map).ToList();
        }

        public Upvote Map(Bot.Votes.Vote source) => new()
        {
            UserId = (int)source.UserId,
            TrackId = source.TrackId,
            AddedAt = source.CreatedAt
        };

        public List<Upvote> Map(List<Bot.Votes.Vote> source)
        {
            return source.Select(Map).ToList();
        }

        public User Map(Bot.Users.User source) => new()
        {
            Id = source.Id.ToString(),
            FirstName = source.FirstName
        };

        public List<User> Map(List<Bot.Users.User> source)
        {
            return source.Select(Map).ToList();
        }

        public SpotifyAccessToken Map(Spotify.Authorization.AuthorizationToken source) => new()
        {
            AccessToken = source.AccessToken,
            TokenType = source.TokenType,
            ExpiresIn = source.ExpiresIn,
            Scope = source.Scope,
            RefreshToken = source.RefreshToken,
            CreatedAt = source.CreatedAt
        };
    }
}
