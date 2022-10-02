using Microsoft.AspNetCore.Http;

namespace SpotiBot.Bot.WebApp
{
    public interface IWebAppValidationService
    {
        bool IsRequestFromTelegram(IQueryCollection queryCollection);
    }
}