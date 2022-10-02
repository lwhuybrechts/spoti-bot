using Microsoft.AspNetCore.Http;
using SpotiBot.Bot.WebApp.Models;

namespace SpotiBot.Bot.WebApp
{
    public interface IMapper
    {
        WebAppInitData Map(IQueryCollection queryCollection);
    }
}