using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.Users
{
    public interface IUserService
    {
        Task<List<User>> Get(List<long> ids);
        Task<User> GetAdmin(UpdateDto updateDto);
        Task<User> SaveUser(UpdateDto updateDto);
    }
}