﻿using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot.Upvotes
{
    public interface IUpvoteService
    {
        bool IsUpvoteCallback(CallbackQuery callbackQuery);
        Task<BotResponseCode> TryHandleUpvote(UpdateDto updateDto);
    }
}