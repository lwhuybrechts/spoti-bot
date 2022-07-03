using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Spoti_bot.Library.Exceptions;
using Sentry;
using Microsoft.Extensions.Options;

namespace Spoti_bot
{
    public class WebAppTest
    {
        private readonly Library.Options.SentryOptions _sentryOptions;

        public WebAppTest(IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(WebAppTest))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Content = "<html><head><script src=\"https://telegram.org/js/telegram-web-app.js\"></script></head><body><h1>WebAppTestje</h1><p></p></body></html>",
                        ContentType = "text/html"
                    };
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    throw;
                }
            }
        }
    }
}
