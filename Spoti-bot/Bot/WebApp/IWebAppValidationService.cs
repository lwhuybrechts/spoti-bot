using Microsoft.AspNetCore.Http;

namespace Spoti_bot.Bot.WebApp
{
    public interface IWebAppValidationService
    {
        bool IsRequestFromTelegram(IQueryCollection queryCollection);
    }
}