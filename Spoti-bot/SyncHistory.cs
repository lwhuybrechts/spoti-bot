using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Library.Exceptions;
using SpotiBot.Spotify.Tracks.SyncHistory;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpotiBot
{
    public class SyncHistory
    {
        private const string _jsonFileName = "chathistory.json";
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public SyncHistory(ISyncHistoryService syncHistoryService, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _syncHistoryService = syncHistoryService;
            _sentryOptions = sentryOptions.Value;
        }

        [Function(nameof(SyncHistory))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // Read the chat history json from a file.
                    var jsonString = File.ReadAllText(_jsonFileName);
                    
                    // Update the track data.
                    var count = await _syncHistoryService.SyncTracksFromJson(jsonString);

                    return new OkObjectResult($"Success: {count} tracks synced based on chat history.");
                }
                catch (FileNotFoundException exception)
                {
                    SentrySdk.CaptureException(exception);
                    return new OkObjectResult($"Reading json file failed, make sure {_jsonFileName} exists in project directory.");
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
