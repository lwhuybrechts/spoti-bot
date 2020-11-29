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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Spoti_bot.Library;
using Spoti_bot.Bot.HandleUpdate;

namespace Spoti_bot
{
    public class Update
    {
        private readonly IHandleMessageService _handleMessageService;
        private readonly IHandleCallbackQueryService _handleCallbackQueryService;
        private readonly IHandleInlineQueryService _handleInlineQueryService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Update(
            IHandleMessageService handleMessageService,
            IHandleCallbackQueryService handleCallbackQueryService,
            IHandleInlineQueryService handleInlineQueryService,
            IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _handleMessageService = handleMessageService;
            _handleCallbackQueryService = handleCallbackQueryService;
            _handleInlineQueryService = handleInlineQueryService;
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

                    // Only handle updates on certain conditions.
                    if (!ShouldHandle(update))
                        return new OkObjectResult(BotResponseCode.NoAction);

                    // Check if we can do something with the text message.
                    var messageResponseCode = await _handleMessageService.TryHandleMessage(update);
                    if (messageResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(messageResponseCode);

                    // Check if we can do something with the callback query.
                    // Callback queries are sent when the user clicks a button.
                    var callbackQueryResponseCode = await _handleCallbackQueryService.TryHandleCallbackQuery(update);
                    if (callbackQueryResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(callbackQueryResponseCode);

                    // Check if we can do something with the inline query.
                    // Inline queries are sent when the user types something behind the bot username tag: @Spotibot query
                    var inlineQueryResponseCode = await _handleInlineQueryService.TryHandleInlineQuery(update);
                    if (inlineQueryResponseCode != BotResponseCode.NoAction)
                        return new OkObjectResult(inlineQueryResponseCode);

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

        private static bool ShouldHandle(Telegram.Bot.Types.Update update)
        {
            // Always handle inline queries.
            if (update?.Type == Telegram.Bot.Types.Enums.UpdateType.InlineQuery)
                return true;
            
            Telegram.Bot.Types.Enums.ChatType? chatType = GetChatType(update);

            if (!chatType.HasValue)
                return false;

            switch (chatType.Value)
            {
                // The bot only handles updates in groups.
                case Telegram.Bot.Types.Enums.ChatType.Group:
                case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                    return true;
                case Telegram.Bot.Types.Enums.ChatType.Channel:
                case Telegram.Bot.Types.Enums.ChatType.Private:
                default:
                    return false;
            }
        }

        private static Telegram.Bot.Types.Enums.ChatType? GetChatType(Telegram.Bot.Types.Update update)
        {
            // Try to get the chat type from the message.
            return update?.Message?.Chat?.Type
                // Try to get the chat type from the callback query's message.
                ?? update?.CallbackQuery?.Message?.Chat?.Type;
        }
    }
}
