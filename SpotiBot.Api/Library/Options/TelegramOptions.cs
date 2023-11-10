using System.ComponentModel.DataAnnotations;

namespace SpotiBot.Api.Library.Options
{
    public class TelegramOptions
    {
        [Required]
        public string AccessToken { get; set; }
        
        [Required]
        public string BotUserName { get; set; }
    }
}
