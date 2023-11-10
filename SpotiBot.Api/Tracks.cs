using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using System.Linq;
using SpotiBot.Data.Services;
using SpotiBot.Library.Enums;
using SpotiBot.Api.Library.Extensions;
using SpotiBot.Api.Library.Exceptions;

namespace SpotiBot.Api
{
    public class Tracks
    {
        private readonly ITrackService _trackService;
        private readonly SpotiBot.Library.ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Tracks(ITrackService trackService, SpotiBot.Library.ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _trackService = trackService;
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
                    var tracks = await _trackService.GetByPlaylist(playlistId);

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
