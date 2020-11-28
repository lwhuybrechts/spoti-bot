using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Spoti_bot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using AutoMapper;
using Spoti_bot.Spotify.Data.Tracks;

namespace Spoti_bot
{
    public class Tracks
    {
        private readonly ITrackRepository _trackRepository;
        private readonly IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Tracks(ITrackRepository trackRepository, IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _trackRepository = trackRepository;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Tracks))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // Get the tracks from storage.
                    var tracks = await _trackRepository.GetAll();

                    // Map the tracks to api models.
                    var apiTracks = _mapper.Map<ApiModels.Track>(tracks);
                    
                    return new OkObjectResult(apiTracks);
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
