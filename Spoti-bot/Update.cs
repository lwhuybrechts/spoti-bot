using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Sentry;
using System.IO;
using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Newtonsoft.Json;
using Spoti_bot.Bot.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Spoti_bot.Bot;

namespace Spoti_bot
{
    public class Update
    {
        private readonly IHandleMessageService _handleMessageService;
        private readonly IHandleCallbackQueryService _handleCallbackQueryService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Update(IHandleMessageService handleMessageService, IHandleCallbackQueryService handleCallbackQueryService, IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _handleMessageService = handleMessageService;
            _handleCallbackQueryService = handleCallbackQueryService;
            _sentryOptions = sentryOptions.Value;
        }

        [FunctionName(nameof(Update))]
        public async Task<IStatusCodeActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest httpRequest)
        {
            var requestBody = await GetRequestBody(httpRequest);

            // Setup exception handling, add to request body so we have all the info we need.
            using (new SentryExceptionHandler(_sentryOptions.Dsn, requestBody))
            {
                try
                {
                    var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(requestBody);

                    // Check if we can do something with the text message.
                    var messageResponseCode = await _handleMessageService.TryHandleMessage(update);
                    if (messageResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(messageResponseCode);

                    // CallbackQueries are responses to a message the bot has sent.
                    var callbackQueryResponseCode = await _handleCallbackQueryService.TryHandleCallbackQuery(update);
                    if (callbackQueryResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(callbackQueryResponseCode);

                    return new OkObjectResult(BotResponseCode.NoAction);
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);

                    // Don't send a message to the chat, since it can create a lot of spam.

                    return new OkObjectResult(BotResponseCode.ExceptionHandled);
                }
            }
        }

        private async Task<string> GetRequestBody(HttpRequest httpRequest)
        {
            try
            {
                using var streamReader = new StreamReader(httpRequest.Body);
                return await streamReader.ReadToEndAsync();
            }
            catch (Exception)
            {
                // Catch this, since we don't have exception handling yet.
                return "";
            }
        }
    }
}
