using Microsoft.AspNetCore.Http;
using SpotiBot.Api.Bot.WebApp.Models;

namespace SpotiBot.Api.Bot.WebApp
{
    public interface IMapper
    {
        WebAppInitData Map(IQueryCollection queryCollection);
    }
}