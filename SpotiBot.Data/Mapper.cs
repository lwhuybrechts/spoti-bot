using SpotiBot.Data.Models;
using SpotiBot.Library.Enums;
using System.Collections.Generic;
using System.Linq;

namespace SpotiBot.Data
{
    internal class Mapper : IMapper
    {
        public Library.BusinessModels.Spotify.LoginRequest Map(LoginRequest source)
        {
            return new(
                source.Id,
                source.GroupChatId,
                source.PrivateChatId,
                source.UserId,
                source.TrackId,
                (LoginRequestReason)source.Reason,
                source.ExpiresAt
            );
        }

        public LoginRequest Map(Library.BusinessModels.Spotify.LoginRequest source)
        {
            return new LoginRequest
            {
                Id = source.Id,
                GroupChatId = source.GroupChatId,
                PrivateChatId = source.PrivateChatId,
                UserId = source.UserId,
                TrackId = source.TrackId,
                Reason = (int)source.Reason,
                ExpiresAt = source.ExpiresAt
            };
        }

        public Library.BusinessModels.Spotify.AuthorizationToken Map(AuthorizationToken source)
        {
            return new(
                source.UserId,
                source.AccessToken,
                source.TokenType,
                source.ExpiresIn,
                source.Scope,
                source.RefreshToken,
                source.CreatedAt
            );
        }

        public AuthorizationToken Map(Library.BusinessModels.Spotify.AuthorizationToken source)
        {
            return new AuthorizationToken
            {
                UserId = source.UserId,
                AccessToken = source.AccessToken,
                TokenType = source.TokenType,
                ExpiresIn = source.ExpiresIn,
                Scope = source.Scope,
                RefreshToken = source.RefreshToken,
                CreatedAt = source.CreatedAt
            };
        }

        public List<Library.BusinessModels.Bot.User> Map(List<User> source)
        {
            return source.Select(Map).ToList();
        }

        public Library.BusinessModels.Bot.User Map(User source)
        {
            return new(
                source.Id,
                source.FirstName,
                source.LastName,
                source.UserName,
                source.LanguageCode
            );
        }

        public User Map(Library.BusinessModels.Bot.User source)
        {
            return new User
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                UserName = source.UserName,
                LanguageCode = source.LanguageCode
            };
        }

        public Library.BusinessModels.Bot.Chat Map(Chat source)
        {
            return new(
                source.Id,
                source.Title,
                source.PlaylistId,
                source.AdminUserId,
                (ChatType)source.Type
            );
        }

        public Chat Map(Library.BusinessModels.Bot.Chat source)
        {
            return new Chat
            {
                Id = source.Id,
                Title = source.Title,
                PlaylistId = source.PlaylistId,
                AdminUserId = source.AdminUserId,
                Type = (int)source.Type
            };
        }

        public List<Library.BusinessModels.Bot.Vote> Map(List<Vote> source)
        {
            return source.Select(Map).ToList();
        }

        public Library.BusinessModels.Bot.Vote Map(Vote source)
        {
            return new(
                source.PlaylistId,
                source.CreatedAt,
                (VoteType)source.Type,
                source.TrackId,
                source.UserId
            );
        }

        public Vote Map(Library.BusinessModels.Bot.Vote source)
        {
            return new Vote
            {
                PlaylistId = source.PlaylistId,
                CreatedAt = source.CreatedAt,
                Type = (int)source.Type,
                TrackId = source.TrackId,
                UserId = source.UserId
            };
        }

        public Library.BusinessModels.Spotify.Playlist Map(Playlist source)
        {
            return new(
                source.Id,
                source.Name
            );
        }

        public Playlist Map(Library.BusinessModels.Spotify.Playlist source)
        {
            return new Playlist
            {
                Id = source.Id,
                Name = source.Name
            };
        }

        public List<Library.BusinessModels.Spotify.Track> Map(List<Track> source)
        {
            return source.Select(Map).ToList();
        }

        public Library.BusinessModels.Spotify.Track Map(Track source)
        {
            return new(
                source.Id,
                source.PlaylistId,
                source.Name,
                source.FirstArtistName,
                source.AlbumName,
                source.AddedByTelegramUserId,
                source.CreatedAt,
                (TrackState)source.State
            );
        }

        public List<Track> Map(List<Library.BusinessModels.Spotify.Track> source)
        {
            return source.Select(Map).ToList();
        }

        public Track Map(Library.BusinessModels.Spotify.Track source)
        {
            return new Track
            {
                Id = source.Id,
                PlaylistId = source.PlaylistId,
                Name = source.Name,
                FirstArtistName = source.FirstArtistName,
                AlbumName = source.AlbumName,
                AddedByTelegramUserId = source.AddedByTelegramUserId,
                CreatedAt = source.CreatedAt,
                State = (int)source.State
            };
        }
    }
}
