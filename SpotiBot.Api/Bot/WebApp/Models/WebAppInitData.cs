using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SpotiBot.Api.Bot.WebApp.Models
{
    /// <summary>
    /// This object contains data that is transferred to the Web App when it is opened. It is empty if the Web App was launched from a keyboard button.
    /// Based on telegram <a href="https://core.telegram.org/bots/webapps#webappinitdata">documentation</a>.
    /// </summary>
    /// <remarks>Added since this model is missing in the Telegram.Bot package.</remarks>
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class WebAppInitData
    {
        /// <summary>
        /// Optional. A unique identifier for the Web App session, required for sending messagesvia the
        /// answerWebAppQuery method.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string QueryId { get; set; }

        /// <summary>
        /// Optional. An object containing data about the current user.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public WebAppUser User { get; set; }

        /// <summary>
        /// Optional. An object containing data about the chat partner of the current user in the chat
        /// where the bot was launched via the attachment menu. Returned only for private chats and only for Web Apps launched via the attachment menu.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public WebAppUser Receiver { get; set; }

        /// <summary>
        /// Optional. An object containing data about the chat where the bot was launched via the attachment menu. Returned for supergroups, channels
        /// and group chats – only for Web Apps launched via the attachment menu.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public WebAppChat Chat { get; set; }

        /// <summary>
        /// Optional. The value of the startattach parameter, passed via link. Only returned for Web Apps when launched from the attachment menu via link.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StartParam { get; set; }

        /// <summary>
        /// Optional. Time in seconds, after which a message can be sent via the answerWebAppQuery method.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? CanSendAfter { get; set; }

        /// <summary>
        /// Unix time when the form was opened.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public long AuthDate { get; set; }

        /// <summary>
        /// A hash of all passed parameters, which the bot server can use to check their validity.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Hash { get; set; }
    }
}
