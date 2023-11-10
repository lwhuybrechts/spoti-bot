using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SpotiBot.Api.Bot.WebApp.Models;
using System.Linq;

namespace SpotiBot.Api.Bot.WebApp
{
    public class Mapper : IMapper
    {
        public WebAppInitData Map(IQueryCollection queryCollection)
        {
            var webAppInitData = new JObject();

            foreach (var key in queryCollection.Keys)
            {
                // In case a key exists multiple times, use only the first value.
                var queryStringValue = queryCollection[key].FirstOrDefault();

                webAppInitData.Add(key, JToken.FromObject(Deserialize(queryStringValue)));
            }

            return webAppInitData.ToObject<WebAppInitData>();
        }

        public static object Deserialize(string queryStringValue)
        {
            // Telegram sends us separate queryString values, of which only complex data types are represented as json serialized objects.
            return IsJson(queryStringValue)
                ? ToObject(JToken.Parse(queryStringValue))
                : queryStringValue;
        }

        public static object ToObject(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Object => token.Children<JProperty>().ToDictionary(property => property.Name, property => ToObject(property.Value)),
                JTokenType.Array => token.Select(ToObject).ToList(),
                _ => ((JValue)token).Value,
            };
        }

        private static bool IsJson(string queryStringValue)
        {
            return new[] { '{', '[' }.Contains(queryStringValue.First()) &&
                new[] { '}', ']' }.Contains(queryStringValue.Last());
        }
    }
}
