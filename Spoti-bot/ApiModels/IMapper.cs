using SpotiBot.Bot.Votes;
using SpotiBot.Spotify.Authorization;
using SpotiApiModels;
using System.Collections.Generic;

namespace SpotiBot.ApiModels
{
    public interface IMapper
    {
        Track Map(Spotify.Tracks.Track source);
        List<Track> Map(List<Spotify.Tracks.Track> source);
        Upvote Map(Vote source);
        List<Upvote> Map(List<Vote> source);
        User Map(Bot.Users.User source);
        List<User> Map(List<Bot.Users.User> source);
        SpotifyAccessToken Map(AuthorizationToken source);
    }
}
