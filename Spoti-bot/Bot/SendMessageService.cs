using Spoti_bot.Bot.Upvotes;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    /// <summary>
    /// This service only exists to override some defaults for sending messages.
    /// TODO: remove, or maybe combine with other services that are dependent on client?
    /// </summary>
    public class SendMessageService : ISendMessageService
    {
        private readonly ITelegramBotClient _telegramBotClient;

        private const ParseMode _defaultParseMode = ParseMode.Markdown;
        private const bool _disableWebPagePreview = true;

        public SendMessageService(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task<int> SendTextMessageAsync(long chatId, string text, ParseMode parseMode = _defaultParseMode, bool disableWebPagePreview = _disableWebPagePreview, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            var message = await _telegramBotClient.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, replyToMessageId: replyToMessageId, replyMarkup: replyMarkup);

            return message.MessageId;
        }

        public Task<int> SendTextMessageAsync(Message messageToRepondTo, string text, ParseMode parseMode = _defaultParseMode, bool disableWebPagePreview = _disableWebPagePreview, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            return SendTextMessageAsync(messageToRepondTo.Chat.Id, text, parseMode, disableWebPagePreview, replyToMessageId, replyMarkup);
        }

        public Task EditMessageTextAsync(long chatId, int messageId, string text, ParseMode parseMode = _defaultParseMode, bool disableWebPagePreview = _disableWebPagePreview, InlineKeyboardMarkup replyMarkup = null)
        {
            return _telegramBotClient.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview, replyMarkup);
        }

        public Task AnswerCallbackQueryAsync(string callbackQueryId)
        {
            return _telegramBotClient.AnswerCallbackQueryAsync(callbackQueryId);
        }
    }
}
