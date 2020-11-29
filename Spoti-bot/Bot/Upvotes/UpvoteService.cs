using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Spotify;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot.Upvotes
{
    public class UpvoteService : IUpvoteService
    {
        public const string ButtonText = "👍";

        private readonly ISendMessageService _sendMessageService;
        private readonly ISpotifyLinkHelper _spotifyLinkHelper;
        private readonly IUpvoteTextHelper _upvoteTextHelper;
        private readonly IUpvoteRepository _upvoteRepository;

        public UpvoteService(
            ISendMessageService sendMessageService,
            ISpotifyLinkHelper spotifyLinkHelper,
            IUpvoteTextHelper upvoteTextHelper,
            IUpvoteRepository upvoteRepository)
        {
            _sendMessageService = sendMessageService;
            _spotifyLinkHelper = spotifyLinkHelper;
            _upvoteTextHelper = upvoteTextHelper;
            _upvoteRepository = upvoteRepository;
        }

        /// <summary>
        /// Check if a callback query is an upvote callback.
        /// </summary>
        /// <param name="callbackQuery">The callback query to check.</param>
        /// <returns>True if the callback query is an upvote callback.</returns>
        public bool IsUpvoteCallback(CallbackQuery callbackQuery)
        {
            return callbackQuery.Data.Equals(ButtonText);
        }

        /// <summary>
        /// Check if a message contains an upvote callback and if so, handle it.
        /// </summary>
        /// <param name="callbackQuery">The callback query to handle.</param>
        public async Task<BotResponseCode> TryHandleUpvote(CallbackQuery callbackQuery)
        {
            if (!IsUpvoteCallback(callbackQuery))
                return BotResponseCode.NoAction;

            var trackId = await GetTrackId(callbackQuery);

            // Every callback query should have a trackId.
            if (string.IsNullOrEmpty(trackId))
                throw new TrackIdNullException();

            var upvoteResponseCode = await HandleUpvote(callbackQuery, trackId);

            // Let telegram know the callback query has been handled.
            await AnswerCallback(callbackQuery);

            return upvoteResponseCode;
        }

        public InlineKeyboardButton CreateUpvoteButton()
        {
            return InlineKeyboardButton.WithCallbackData(ButtonText);
        }

        /// <summary>
        /// Get the trackId from the callback query.
        /// </summary>
        private async Task<string> GetTrackId(CallbackQuery callbackQuery)
        {
            // Get the message with the trackId.
            var trackMessageText = callbackQuery.Message.ReplyToMessage.Text;

            return await _spotifyLinkHelper.ParseTrackId(trackMessageText);
        }

        /// <summary>
        /// Upvote a track, or if it was already upvoted remove the upvote.
        /// </summary>
        private async Task<BotResponseCode> HandleUpvote(CallbackQuery callbackQuery, string trackId)
        {
            var upvote = new Upvote
            {
                TrackId = trackId,
                UserId = callbackQuery.From.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var text = GetTextMessageWithTextLinks(callbackQuery.Message);

            var existingUpvote = await _upvoteRepository.Get(upvote);

            return existingUpvote == null
                ? await Upvote(callbackQuery, upvote, text)
                : await Downvote(callbackQuery, existingUpvote, text);
        }

        private async Task<BotResponseCode> Upvote(CallbackQuery callbackQuery, Upvote upvote, string text)
        {
            // Save the upvote in storage.
            await _upvoteRepository.Upsert(upvote);

            // Increment upvote in the original message.
            var newText = _upvoteTextHelper.IncrementUpvote(text);
            await EditOriginalMessage(callbackQuery.Message, newText);
            
            return BotResponseCode.UpvoteHandled;
        }

        private async Task<BotResponseCode> Downvote(CallbackQuery callbackQuery, Upvote existingUpvote, string text)
        {
            // Decrement upvote in the original message.
            var newText = _upvoteTextHelper.DecrementUpvote(text);
            await EditOriginalMessage(callbackQuery.Message, newText);

            // Delete the upvote from storage.
            await _upvoteRepository.Delete(existingUpvote);

            return BotResponseCode.DownvoteHandled;
        }

        // TODO: move to a central place?
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
        /// <param name="message">The message to edit.</param>
        /// <param name="newText">The new text to replace the message text with.</param>
        private async Task EditOriginalMessage(Message message, string newText)
        {
            await _sendMessageService.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                newText,
                replyMarkup: message.ReplyMarkup);
        }

        /// <summary>
        /// Let telegram know the callback query has been handled.
        /// </summary>
        /// <param name="callbackQuery">The callback query.</param>
        private async Task AnswerCallback(CallbackQuery callbackQuery)
        {
            try
            {
                await _sendMessageService.AnswerCallbackQueryAsync(callbackQuery.Id);
            }
            catch (InvalidParameterException)
            {
                // This may crash when the callback query is too old, just ignore it.
                return;
            }
        }
    }
}
