using Spoti_bot.Bot.Upvotes;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Spoti_bot.Bot.HandleUpdate
{
    public class HandleCallbackQueryService : IHandleCallbackQueryService
    {
        private readonly IUpvoteService _upvoteService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IUserService _userService;

        public HandleCallbackQueryService(IUpvoteService upvoteService, ISendMessageService sendMessageService, IUserService userService)
        {
            _upvoteService = upvoteService;
            _sendMessageService = sendMessageService;
            _userService = userService;
        }

        public async Task<BotResponseCode> TryHandleCallbackQuery(Telegram.Bot.Types.Update update)
        {
            // If the bot can't do anything with the update's callback query, we're done.
            if (!CanHandleCallbackQuery(update))
                return BotResponseCode.NoAction;

            // Check if an upvote should be handled and if so handle it.
            var upvoteResponseCode = await _upvoteService.TryHandleUpvote(update.CallbackQuery);
            if (upvoteResponseCode != BotResponseCode.NoAction)
            {
                // Save users that upvoted.
                await _userService.SaveUser(update.CallbackQuery.From);

                // Let telegram know the callback query has been handled.
                await AnswerCallback(update.CallbackQuery, upvoteResponseCode);

                return upvoteResponseCode;
            }

            // This should never happen.
            throw new CallbackQueryNotHandledException();
        }

        /// <summary>
        /// Check if the bot can handle the update's callback query. 
        /// </summary>
        /// <param name="update">The update to check.</param>
        /// <returns>True if the bot can handle the callback query.</returns>
        private bool CanHandleCallbackQuery(Telegram.Bot.Types.Update update)
        {
            // Check if we have all the data we need.
            if (update == null ||
                // Filter everything but callback queries.
                update.Type != UpdateType.CallbackQuery ||
                update.CallbackQuery == null ||
                string.IsNullOrEmpty(update.CallbackQuery.Id) ||
                update.CallbackQuery.From == null ||
                // We only support callback queries on text messages.
                update.CallbackQuery.Message == null ||
                update.CallbackQuery.Message.Type != MessageType.Text ||
                string.IsNullOrEmpty(update.CallbackQuery.Message.Text) ||
                // We only support callback queries on messages that are a reply to another text message.
                update.CallbackQuery.Message.ReplyToMessage == null ||
                update.CallbackQuery.Message.ReplyToMessage.Type != MessageType.Text ||
                string.IsNullOrEmpty(update.CallbackQuery.Message.ReplyToMessage.Text))
                return false;

            if (!_upvoteService.IsUpvoteCallback(update.CallbackQuery))
                return false;

            return true;
        }

        /// <summary>
        /// Let telegram know the callback query has been handled.
        /// </summary>
        /// <param name="callbackQuery">The callback query.</param>
        private async Task AnswerCallback(CallbackQuery callbackQuery, BotResponseCode botResponseCode)
        {
            string text = null;
            switch (botResponseCode)
            {
                case BotResponseCode.UpvoteHandled:
                    text = "Upvoted";
                    break;
                case BotResponseCode.DownvoteHandled:
                    text = "Removed upvote";
                    break;
            }

            try
            {
                await _sendMessageService.AnswerCallbackQuery(callbackQuery.Id, text);
            }
            catch (InvalidParameterException exception)
            {
                // This may crash when the callback query is too old, just ignore it.
                if (exception?.Message == "query is too old and response timeout expired or query ID is invalid")
                    return;

                throw;
            }
        }
    }
}
