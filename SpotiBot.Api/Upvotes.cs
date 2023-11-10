using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using System.Linq;
using SpotiBot.Library.Enums;
using SpotiBot.Api.Library.Exceptions;

namespace SpotiBot.Api
{
    public class Upvotes
    {
        private readonly Data.Services.IVoteService _voteService;
        private readonly SpotiBot.Library.ApiModels.IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Upvotes(Data.Services.IVoteService voteService, SpotiBot.Library.ApiModels.IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _voteService = voteService;
            _mapper = mapper;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Upvotes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    // For now, only return upvotes from the main playlist.
                    const string playlistId = "2tnyzyB8Ku9XywzAYNjLxj";

                    var votes = await _voteService.Get(playlistId);

                    var upvotes = votes.Where(x => x.Type == VoteType.Upvote).ToList();

                    // Map the upvotes to api models.
                    var apiUpvotes = _mapper.Map(upvotes);

                    return new OkObjectResult(apiUpvotes);
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
