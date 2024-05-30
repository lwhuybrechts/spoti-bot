using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Library.Exceptions;
using SpotiBot.Library.Extensions;
using SpotiBot.Spotify.Tracks;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [Function(nameof(Tracks))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest)
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
