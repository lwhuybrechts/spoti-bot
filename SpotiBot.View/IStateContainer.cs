using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.View
{
    public interface IStateContainer
    {
        AuthorizationToken? SpotifyAccessToken { get; set; }

        event Action? OnChange;
    }
}