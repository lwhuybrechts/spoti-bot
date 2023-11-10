namespace SpotiBot.Library.BusinessModels.Spotify
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Playlist(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
