using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace SpotiBot.Api.Library.Extensions
{
    public static class QueryCollectionExtensions
    {
        public static string GetFirstValue(this IQueryCollection queryCollection, string key)
        {
            return queryCollection.GetFirstValue<string>(key);
        }

        public static T GetFirstValue<T>(this IQueryCollection queryCollection, string key)
        {
            if (!queryCollection.TryGetValue(key, out var values) || !values.Any())
                return default;

            var firstValue = values.First();

            if (string.IsNullOrEmpty(firstValue))
                return default;

            return (T)Convert.ChangeType(firstValue, typeof(T));
        }
    }
}
