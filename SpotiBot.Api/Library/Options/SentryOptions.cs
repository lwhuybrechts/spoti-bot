using System.ComponentModel.DataAnnotations;

namespace SpotiBot.Api.Library.Options
{
    public class SentryOptions
    {
        [Required]
        public string Dsn { get; set; }
    }
}
