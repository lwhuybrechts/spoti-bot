using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Sentry;
using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Spoti_bot.Spotify.Authorization;

namespace Spoti_bot
{
    public class Callback
    {
        public const string SuccessMessage = "Spoti-bot is now authorized, enjoy!";
        public const string ErrorMessage = "Could not authorize Spoti-bot, code is invalid.";

        private readonly IAuthorizationService _spotifyAuthorizationService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Callback(IAuthorizationService spotifyAuthorizationService, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Callback))]
        public async Task<IStatusCodeActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    (string code, string loginRequestId) = GetQueryParameters(httpRequest);

                    // Request and save an AuthorizationToken which we can use to do calls to the spotify api.
                    await _spotifyAuthorizationService.RequestAndSaveAuthorizationToken(code, loginRequestId);

                    // Send a reply that is visible in the browser where the used just logged in.
                    return new OkObjectResult(SuccessMessage);
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);

                    // Send a reply that is visible in the browser where the used just logged in.
                    return new OkObjectResult(ErrorMessage);
                }
            }
        }

        private (string, string) GetQueryParameters(HttpRequest httpRequest)
        {
            // Get the code and state from the callback url.
            var code = GetFromQuery("code", httpRequest);
            var loginRequestId = GetFromQuery("state", httpRequest);

            if (string.IsNullOrEmpty(code))
                throw new QueryParameterNullException(nameof(code));

            if (string.IsNullOrEmpty(loginRequestId))
                throw new QueryParameterNullException(nameof(loginRequestId));

            return (code, loginRequestId);
        }

        private static string GetFromQuery(string key, HttpRequest httpRequest)
        {
            if (httpRequest == null || httpRequest.Query == null)
                return string.Empty;

            return httpRequest.Query.ContainsKey(key)
                ? httpRequest.Query[key].ToString()
                : string.Empty;
        }
    }
}
