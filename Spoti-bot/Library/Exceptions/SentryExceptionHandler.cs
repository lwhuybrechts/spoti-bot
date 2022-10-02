using Sentry;
using System;

namespace SpotiBot.Library.Exceptions
{
    /// <summary>
    /// A wrapper around SentrySdk.Init.
    /// </summary>
    public class SentryExceptionHandler : IDisposable
    {
        private IDisposable _sentryDisposable;

        public SentryExceptionHandler(string dsn, string requestBody = null)
        {
            _sentryDisposable = SentrySdk.Init(options =>
            {
                options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                options.Dsn = dsn;
                options.BeforeSend = sentryEvent =>
                {
                    sentryEvent.SetTag("Application", "spoti-bot");

                    if (!string.IsNullOrEmpty(requestBody))
                        // Include the entire http request body to help with debugging.
                        sentryEvent.SetExtra("requestBody", requestBody);

                    return sentryEvent;
                };
            });
        }

        public void Dispose()
        {
            if (_sentryDisposable != null)
            {
                _sentryDisposable.Dispose();
                _sentryDisposable = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
