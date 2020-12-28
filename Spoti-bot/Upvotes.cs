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
using System.Collections.Generic;
using Spoti_bot.Bot.Votes;
using System.Linq;

namespace Spoti_bot
{
    public class Upvotes
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IMapper _mapper;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Upvotes(IVoteRepository voteRepository, IMapper mapper, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _voteRepository = voteRepository;
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

                    var votes = await _voteRepository.GetAllByPartitionKey(playlistId);

                    var upvotes = votes.Where(x => x.Type == VoteType.Upvote).ToList();

                    // Map the upvotes to api models.
                    var apiUpvotes = _mapper.Map<List<ApiModels.Upvote>>(upvotes);

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
