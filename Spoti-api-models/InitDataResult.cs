namespace SpotiApiModels
{
    public class InitDataResult
    {
        public SpotifyAccessToken SpotifyAccessToken { get; set; }

        public InitDataResult(SpotifyAccessToken spotifyAccessToken)
        {
            SpotifyAccessToken = spotifyAccessToken;
        }
    }
}
