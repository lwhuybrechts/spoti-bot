using Microsoft.AspNetCore.Http;
using Spoti_bot.Bot.WebApp.Models;

namespace Spoti_bot.Bot.WebApp
{
    public interface IMapper
    {
        WebAppInitData Map(IQueryCollection queryCollection);
    }
}