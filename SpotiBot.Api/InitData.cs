using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Api.Bot.WebApp;
using SpotiBot.Api.Bot.WebApp.Models;
using SpotiBot.Api.Library.Exceptions;
using SpotiBot.Data.Services;
using SpotiBot.Library.ApiModels;
using System;
using System.Threading.Tasks;
using IMapper = SpotiBot.Api.Bot.WebApp.IMapper;

namespace SpotiBot.Api
{
    public class InitData
    {
        private readonly Library.Options.SentryOptions _sentryOptions;
        private readonly IWebAppValidationService _webAppValidationService;
        private readonly IAuthorizationTokenService _authorizationTokenService;
        private readonly IMapper _webAppMapper;
        private readonly SpotiBot.Library.ApiModels.IMapper _apiMapper;

        public InitData(
            IOptions<Library.Options.SentryOptions> sentryOptions,
            IWebAppValidationService webAppValidationService,
            IAuthorizationTokenService authorizationTokenService,
            IMapper webAppMapper,
            SpotiBot.Library.ApiModels.IMapper apiMapper)
        {
            _sentryOptions = sentryOptions.Value;
            _webAppValidationService = webAppValidationService;
            _authorizationTokenService = authorizationTokenService;
            _webAppMapper = webAppMapper;
            _apiMapper = apiMapper;
        }

        [FunctionName(nameof(InitData))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    if (!_webAppValidationService.IsRequestFromTelegram(httpRequest.Query))
                        return new BadRequestResult();

                    var webAppInitData = _webAppMapper.Map(httpRequest.Query);

                    if (webAppInitData == null)
                        return new BadRequestResult();

                    if (IsExpired(webAppInitData))
                        return new BadRequestResult();

                    // When telegram did not send us a user, we're done here.
                    if (webAppInitData.User == null)
                        return new NoContentResult();

                    var token = await _authorizationTokenService.Get(webAppInitData.User.Id);

                    // TODO: add fallback to admin token.
                    //if (token == null)
                    //token = await _authorizationTokenRepository.Get(webAppInitData.User.Id);

                    // If we cannot find a token, we're done here.
                    if (token == null)
                        return new NoContentResult();

                    return new OkObjectResult(new InitDataResult(webAppInitData.User.Id, _apiMapper.Map(token)));
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);
                    throw;
                }
            }
        }

        private static bool IsExpired(WebAppInitData webAppInitData)
        {
            // We only handle requests that were sent in the last few minutes.
            return webAppInitData.AuthDate < DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        }
    }
}
