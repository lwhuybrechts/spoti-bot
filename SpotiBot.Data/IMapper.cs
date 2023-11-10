using SpotiBot.Data.Models;
using System.Collections.Generic;

namespace SpotiBot.Data
{
    public interface IMapper
    {
        Library.BusinessModels.Spotify.LoginRequest Map(LoginRequest source);
        LoginRequest Map(Library.BusinessModels.Spotify.LoginRequest source);
        Library.BusinessModels.Spotify.AuthorizationToken Map(AuthorizationToken source);
        AuthorizationToken Map(Library.BusinessModels.Spotify.AuthorizationToken source);
        List<Library.BusinessModels.Bot.User> Map(List<User> source);
        Library.BusinessModels.Bot.User Map(User source);
        User Map(Library.BusinessModels.Bot.User source);
        Library.BusinessModels.Bot.Chat Map(Chat source);
        Chat Map(Library.BusinessModels.Bot.Chat source);
        List<Library.BusinessModels.Bot.Vote> Map(List<Vote> source);
        Library.BusinessModels.Bot.Vote Map(Vote source);
        Vote Map(Library.BusinessModels.Bot.Vote source);
        Library.BusinessModels.Spotify.Playlist Map(Playlist source);
        Playlist Map(Library.BusinessModels.Spotify.Playlist source);
        List<Library.BusinessModels.Spotify.Track> Map(List<Track> source);
        Library.BusinessModels.Spotify.Track Map(Track source);
        List<Track> Map(List<Library.BusinessModels.Spotify.Track> source);
        Track Map(Library.BusinessModels.Spotify.Track source);
    }
}