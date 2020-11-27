using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Sentry;
using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Spoti_bot
{
    public class Callback
    {
        public const string SuccessMessage = "Spoti-bot is now authorized, enjoy!";
        public const string ErrorMessage = "Could not authorize Spoti-bot, code is invalid.";

        private readonly ISpotifyAuthorizationService _spotifyAuthorizationService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Callback(ISpotifyAuthorizationService spotifyAuthorizationService, IOptions<Library.Options.SentryOptions> sentryOptions)
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
                    // Get the code from the callback url.
                    var code = httpRequest.Query.ContainsKey("code") ? httpRequest.Query["code"].ToString() : "";

                    if (string.IsNullOrEmpty(code))
                        throw new CallbackCodeNullException();

                    // Request and save an AuthorizationToken which we can use to do calls to the spotify api.
                    await _spotifyAuthorizationService.RequestAndSaveAuthorizationToken(code);

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
    }
}
