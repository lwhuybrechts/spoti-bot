using Newtonsoft.Json;

namespace Spoti_bot.Bot.WebApp.Models
{
    [JsonConverter(typeof(WebAppChatTypeConverter))]
    public enum WebAppChatType
    {
        Group = 1,
        Supergroup = 2,
        Channel = 3
    }
}
