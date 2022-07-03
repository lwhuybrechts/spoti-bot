using System.Collections.Generic;

namespace Spoti_bot.ApiModels
{
    public interface IMapper
    {
        Track Map(Spotify.Tracks.Track source);
        List<Track> Map(List<Spotify.Tracks.Track> source);
        Upvote Map(Bot.Votes.Vote source);
        List<Upvote> Map(List<Bot.Votes.Vote> source);
        User Map(Bot.Users.User source);
        List<User> Map(List<Bot.Users.User> source);
    }
}
