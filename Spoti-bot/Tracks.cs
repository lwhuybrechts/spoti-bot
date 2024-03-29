using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using SpotiBot.Library.Exceptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using SpotiBot.Spotify.Tracks;
using System.Linq;
using SpotiBot.Library.Extensions;

namespace SpotiBot
{
    public class Tracks
    {
        private readonly ITrackRepository _trackRepository;
        private readonly ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Tracks(ITrackRepository trackRepository, ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
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
                    // For now, only return tracks from the main playlist.
                    const string playlistId = "2tnyzyB8Ku9XywzAYNjLxj";

                    // Get the tracks from storage.
                    var tracks = await _trackRepository.GetAllByPartitionKey(playlistId);

                    // Don't return tracks that are marked as removed.
                    tracks = tracks.Where(x => x.State != TrackState.RemovedByDownvotes).ToList();

                    // Map the tracks to api models.
                    var apiTracks = _mapper.Map(tracks);

                    httpRequest.AddResponseCaching(300);

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
