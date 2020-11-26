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

namespace Spoti_bot
{
    public class Callback
    {
        private readonly ISpotifyAuthorizationService _spotifyAuthorizationService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Callback(ISpotifyAuthorizationService spotifyAuthorizationService, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Callback))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
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

                    // Send a reply.
                    return new OkObjectResult("Spoti-bot is now authorized, enjoy!");
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    return new OkObjectResult("Could not authorize Spoti-bot, code is invalid.");
                }
            }
        }
    }
}
