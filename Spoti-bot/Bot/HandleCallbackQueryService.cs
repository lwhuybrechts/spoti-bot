using Spoti_bot.Bot.Upvotes;
using Spoti_bot.Bot.Users;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace Spoti_bot.Bot
{
    public class HandleCallbackQueryService : IHandleCallbackQueryService
    {
        private readonly IUpvoteService _upvoteService;
        private readonly IUserService _userService;

        public HandleCallbackQueryService(IUpvoteService upvoteService, IUserService userService)
        {
            _upvoteService = upvoteService;
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
    }
}
