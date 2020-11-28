using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spoti_bot.Spotify.Data.Tracks;

namespace Spoti_bot
{
    public class GetTracks
    {
        private readonly ITrackRepository _trackRepository;

        public GetTracks(ITrackRepository trackRepository) {
            _trackRepository = trackRepository;
        }
        
        [FunctionName(nameof(GetTracks))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            // Get the tracks and put them into the response message of the API.
            var responseMessage = await _trackRepository.GetAll();

            return new OkObjectResult(responseMessage);
        }
    }
}
