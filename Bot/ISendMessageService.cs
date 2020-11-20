﻿using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Spoti_bot.Bot
{
    public interface ISendMessageService
    {
        Task SendTextMessageAsync(long chatId, string text, ParseMode parseMode = ParseMode.Markdown, bool disableWebPagePreview = true, int replyToMessageId = 0, IReplyMarkup replyMarkup = null);
        Task EditMessageTextAsync(long chatId, int messageId, string text, ParseMode parseMode = ParseMode.Markdown, bool disableWebPagePreview = true, InlineKeyboardMarkup replyMarkup = null);
        Task AnswerCallbackQueryAsync(string callbackQueryId);
    }
}
