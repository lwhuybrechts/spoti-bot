using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.View
{
    public class StateContainer : IStateContainer
    {
        public event Action? OnChange;

        private AuthorizationToken? _spotifyAccessToken;
        public AuthorizationToken? SpotifyAccessToken
        {
            get => _spotifyAccessToken;
            set
            {
                _spotifyAccessToken = value;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
