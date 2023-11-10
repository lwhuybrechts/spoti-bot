using System.Collections.Generic;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotifyAPI.Web;

namespace SpotiBot.Library.ApiModels
{
    public interface IMapper
    {
        Track Map(BusinessModels.Spotify.Track source);
        List<Track> Map(List<BusinessModels.Spotify.Track> source);
        List<BusinessModels.Spotify.Track> Map(List<Track> tracks, List<FullTrack> apiTracks);
        BusinessModels.Spotify.Track Map(Track track, FullTrack fullTrack);
        Upvote Map(Vote source);
        List<Upvote> Map(List<Vote> source);
        User Map(BusinessModels.Bot.User source);
        List<User> Map(List<BusinessModels.Bot.User> source);
        SpotifyAccessToken Map(AuthorizationToken source);
        AuthorizationToken Map(SpotifyAccessToken source, long userId);
    }
}
