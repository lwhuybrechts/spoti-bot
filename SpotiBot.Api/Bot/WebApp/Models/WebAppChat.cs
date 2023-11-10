using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace SpotiBot.Api.Bot.WebApp.Models
{
    /// <summary>
    /// This object represents a chat.
    /// Based on telegram <a href="https://core.telegram.org/bots/webapps#webappchat">documentation</a>.
    /// </summary>
    /// <remarks>Added since this model is missing in the Telegram.Bot package.</remarks>
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class WebAppChat
    {
        /// <summary>
        /// Unique identifier for this chat.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public long Id { get; set; }

        /// <summary>
        /// Type of chat, can be either "group", "supergroup" or "channel".
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public WebAppChatType Type { get; set; }

        /// <summary>
        /// Title of the chat.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Optional. Username of the chat.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserName { get; set; }

        /// <summary>
        /// Optional. URL of the chat’s photo. The photo can be in .jpeg or .svg formats. Only returned for Web Apps launched from the attachment menu.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PhotoUrl { get; set; }
    }
}