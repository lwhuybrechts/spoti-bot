using Spoti_bot.Bot.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public class HandleCallbackQueryService : IHandleCallbackQueryService
    {
        private readonly ISendMessageService _sendMessageService;
        private readonly IUpvoteHelper _upvoteHelper;

        public HandleCallbackQueryService(ISendMessageService sendMessageService, IUpvoteHelper upvoteHelper)
        {
            _sendMessageService = sendMessageService;
            _upvoteHelper = upvoteHelper;
        }

        public async Task<bool> TryHandleCallbackQuery(Telegram.Bot.Types.Update update)
        {
            // If the bot can't do anything with the update's callback query, we're done.
            if (!CanHandleCallbackQuery(update))
                return false;

            await HandleUpvote(update.CallbackQuery);
            
            return true;
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
                update.CallbackQuery.Message == null ||
                // Filter everything but text messages.
                update.CallbackQuery.Message.Type != MessageType.Text ||
                string.IsNullOrEmpty(update.CallbackQuery.Message.Text))
                return false;

            if (!IsUpvoteCallback(update))
                return false;

            return true;
        }

        /// <summary>
        /// Check if an update is an upvote callback.
        /// </summary>
        /// <param name="update">The update to check.</param>
        /// <returns>True if the update is an upvote callback.</returns>
        private static bool IsUpvoteCallback(Telegram.Bot.Types.Update update)
        {
            return update.CallbackQuery.Data.Equals(UpvoteHelper.ButtonText);
        }

        /// <summary>
        /// Add or increment an upvote in the original message.
        /// </summary>
        /// <param name="callbackQuery">The callback query to handle.</param>
        /// <returns>The original message with an updated upvote.</returns>
        private async Task HandleUpvote(CallbackQuery callbackQuery)
        {
            var newText = GetTextMessageWithTextLinks(callbackQuery.Message);

            if (!_upvoteHelper.EndsWithUpvote(newText))
                newText = _upvoteHelper.AddUpvote(newText);
            else
                newText = _upvoteHelper.IncrementUpvote(newText);

            await EditOriginalMessage(callbackQuery.Message, newText);
            await AnswerCallback(callbackQuery);
        }

        /// <summary>
        /// Get the original text and re-add the links to it.
        /// We need to do this since the telegram library we use doesn't offer a way to pass the entities.
        /// </summary>
        /// <param name="textMessage">The message we want the text from.</param>
        /// <returns>The text with all links added to it.</returns>
        private string GetTextMessageWithTextLinks(Message textMessage)
        {
            var text = textMessage.Text;

            foreach (var textLinkEntity in textMessage.Entities.Where(x => x.Type == MessageEntityType.TextLink))
            {
                var firstPart = text.Substring(0, textLinkEntity.Offset);
                var linkText = text.Substring(firstPart.Length, textLinkEntity.Length);
                var lastPart = text.Substring(firstPart.Length + linkText.Length);
                
                text = $"{firstPart}[{linkText}]({textLinkEntity.Url}){lastPart}";
            }

            return text;
        }

        /// <summary>
        /// Edit the message that triggered the callback query.
        /// </summary>
        /// <param name="message">The message to edit..</param>
        /// <param name="newText">The new text to replace the message text with.</param>
        private async Task EditOriginalMessage(Message message, string newText)
        {
            await _sendMessageService.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                newText,
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(UpvoteHelper.ButtonText)));
        }

        /// <summary>
        /// Let telegram know the callback query has been handled.
        /// </summary>
        /// <param name="callbackQuery">The callback query.</param>
        private Task AnswerCallback(CallbackQuery callbackQuery)
        {
            return _sendMessageService.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
    }
}
