namespace SpotiBot.Library.ApiModels
{
    public class InitDataResult
    {
        public long UserId { get; set; }
        public SpotifyAccessToken SpotifyAccessToken { get; set; }

        public InitDataResult(long userId, SpotifyAccessToken spotifyAccessToken)
        {
            UserId = userId;
            SpotifyAccessToken = spotifyAccessToken;
        }
    }
}
