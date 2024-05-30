using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Library.Exceptions;
using System;
using System.IO;

namespace SpotiBot
{
    public class WebApp
    {
        private readonly Library.Options.SentryOptions _sentryOptions;

        public WebApp(IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _sentryOptions = sentryOptions.Value;
        }

        [Function(nameof(WebApp))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest, ExecutionContext context)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    var webAppPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", "WebAppFiles", "index.html");

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
