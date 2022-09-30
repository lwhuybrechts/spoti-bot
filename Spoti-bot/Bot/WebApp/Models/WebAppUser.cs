using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spoti_bot.Bot.WebApp.Models
{
    /// <summary>
    /// This object contains the data of the Web App user.
    /// Based on telegram <a href="https://core.telegram.org/bots/webapps#webappuser">documentation</a>.
    /// </summary>
    /// <remarks>Added since this model is missing in the Telegram.Bot package.</remarks>
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class WebAppUser
    {
        /// <summary>
        /// A unique identifier for the user or bot.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public long Id { get; set; }

        /// <summary>
        /// Optional. True, if this user is a bot. Returns in the receiver field only.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? IsBot { get; set; }

        /// <summary>
        /// First name of the user or bot.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string FirstName { get; set; }

        /// <summary>
        /// Optional. Last name of the user or bot.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? LastName { get; set; }

        /// <summary>
        /// Optional. Username of the user or bot.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Username { get; set; }

        /// <summary>
        /// Optional. IETF language tag of the user's language. Returns in user field only.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? LanguageCode { get; set; }

        /// <summary>
        /// Optional. True, if this user is a Telegram Premium user.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? IsPremium { get; set; }

        /// <summary>
        /// Optional. URL of the user’s profile photo. The photo can be in .jpeg or .svg formats. Only returned for Web Apps launched from the attachment menu.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? PhotoUrl { get; set; }
    }
}
