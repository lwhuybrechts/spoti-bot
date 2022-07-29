using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Spoti_bot.Library.Exceptions;
using Sentry;
using Microsoft.Extensions.Options;
using System.IO;

namespace Spoti_bot
{
    public class WebApp
    {
        private readonly Library.Options.SentryOptions _sentryOptions;

        public WebApp(IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(WebApp))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest, ExecutionContext context)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    var webAppPath = Path.Combine(context.FunctionAppDirectory, "WebAppFiles", "index.html");

                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Content = File.ReadAllText(webAppPath),
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
