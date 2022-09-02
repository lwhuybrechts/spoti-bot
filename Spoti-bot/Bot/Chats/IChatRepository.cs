﻿using System.Threading.Tasks;

namespace Spoti_bot.Bot.Chats
{
    public interface IChatRepository
    {
        Task<Chat> Get(long rowKey, string partitionKey = "");
        Task<Chat> Get(Chat item);
        Task<Chat> Upsert(Chat item);
        Task Delete(Chat item);
    }
}