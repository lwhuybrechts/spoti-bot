using System.Threading.Tasks;
using Telegram.Bot;
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

        public async Task SendTextMessageAsync(long chatId, string text, ParseMode parseMode = _defaultParseMode, bool disableWebPagePreview = _disableWebPagePreview, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            await _telegramBotClient.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, replyToMessageId: replyToMessageId, replyMarkup: replyMarkup);
        }

        public async Task EditMessageTextAsync(long chatId, int messageId, string text, ParseMode parseMode = _defaultParseMode, bool disableWebPagePreview = _disableWebPagePreview, InlineKeyboardMarkup replyMarkup = null)
        {
            await _telegramBotClient.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview, replyMarkup);
        }

        public async Task AnswerCallbackQueryAsync(string callbackQueryId)
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(callbackQueryId);
        }
    }
}
