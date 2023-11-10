using Microsoft.AspNetCore.Http;

namespace SpotiBot.Api.Bot.WebApp
{
    public interface IWebAppValidationService
    {
        bool IsRequestFromTelegram(IQueryCollection queryCollection);
    }
}