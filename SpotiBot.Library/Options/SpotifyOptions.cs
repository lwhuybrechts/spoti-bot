using System.ComponentModel.DataAnnotations;

namespace SpotiBot.Library.Options
{
    public class SpotifyOptions
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
        
        [Required]
        public string Secret { get; set; } = string.Empty;
    }
}
