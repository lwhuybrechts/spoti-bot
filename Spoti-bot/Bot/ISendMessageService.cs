using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpotiBot.Bot
{
    public interface ISendMessageService
    {
        Task<int> SendTextMessage(long chatId, string text, ParseMode parseMode = ParseMode.Markdown, bool disableWebPagePreview = true, int replyToMessageId = 0, IReplyMarkup replyMarkup = null);
        Task EditMessageText(long chatId, int messageId, string text, ParseMode parseMode = ParseMode.Markdown, bool disableWebPagePreview = true, InlineKeyboardMarkup replyMarkup = null);
        Task AnswerCallbackQuery(string callbackQueryId, string text = null, string url = null);
        Task AnswerInlineQuery(string inlineQueryId, IEnumerable<InlineQueryResultArticle> results, string switchPmText = null, string switchPmParameter = null);
    }
}