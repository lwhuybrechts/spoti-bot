using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;

namespace SpotiBot.Api.Library.Extensions
{
    public static class HttpRequestExtensions
    {
        public static void AddResponseCaching(this HttpRequest httpRequest, int durationInSeconds)
        {
            httpRequest.HttpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(durationInSeconds),
                NoStore = false,
                Public = true
            };
        }
    }
}