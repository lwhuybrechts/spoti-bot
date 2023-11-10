using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Api.Bot.Users
{
    public class UserService : IUserService
    {
        private readonly Data.Services.IUserService _userService;
        private readonly IMapper _mapper;

        public UserService(Data.Services.IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public Task<List<User>> Get(List<long> ids)
        {
            return _userService.Get(ids);
        }

        public Task<User> GetAdmin(UpdateDto updateDto)
        {
            return _userService.Get(updateDto.Chat.AdminUserId);
        }

        public Task<User> SaveUser(UpdateDto updateDto)
        {
            var user = updateDto.User;
            if (updateDto.User == null)
                user = _mapper.Map(updateDto.ParsedUser);

            var chatId = updateDto.Chat?.Id ?? updateDto.ParsedChat.Id;

            return _userService.SaveUser(user, chatId);
        }
    }
}
