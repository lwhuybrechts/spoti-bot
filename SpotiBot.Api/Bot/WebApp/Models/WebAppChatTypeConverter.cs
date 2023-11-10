using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SpotiBot.Api.Bot.WebApp.Models
{
    internal class WebAppChatTypeConverter : JsonConverter<WebAppChatType>
    {
        public override void WriteJson(JsonWriter writer, WebAppChatType value, JsonSerializer serializer) =>
            writer.WriteValue(GetStringValue(value));

        public override WebAppChatType ReadJson(JsonReader reader, Type objectType, WebAppChatType existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            GetEnumValue(JToken.ReadFrom(reader).Value<string>());

        private static readonly IReadOnlyDictionary<string, WebAppChatType> StringToEnum = new Dictionary<string, WebAppChatType>
        {
            { "group", WebAppChatType.Group },
            { "channel", WebAppChatType.Channel },
            { "supergroup", WebAppChatType.Supergroup }
        };

        private static readonly IReadOnlyDictionary<WebAppChatType, string> EnumToString = new Dictionary<WebAppChatType, string>
        {
            { WebAppChatType.Group, "group" },
            { WebAppChatType.Channel, "channel" },
            { WebAppChatType.Supergroup, "supergroup" }
        };

        private static WebAppChatType GetEnumValue(string value) =>
            StringToEnum.TryGetValue(value, out var enumValue)
                ? enumValue
                : 0;

        private static string GetStringValue(WebAppChatType value) =>
            EnumToString.TryGetValue(value, out var stringValue)
                ? stringValue
                : "unknown";
    }
}
