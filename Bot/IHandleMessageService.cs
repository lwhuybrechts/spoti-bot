﻿using System.Threading.Tasks;

namespace Spoti_bot.Bot
{
    public interface IHandleMessageService
    {
        Task<bool> TryHandleMessage(Telegram.Bot.Types.Update update);
    }
}
