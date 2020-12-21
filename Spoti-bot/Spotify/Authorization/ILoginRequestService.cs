﻿using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface ILoginRequestService
    {
        Task<LoginRequest> Create(long userId, long chatId);
        Task<LoginRequest> Get(string id);
        Task Delete(LoginRequest loginRequest);
    }
}