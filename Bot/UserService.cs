using AutoMapper;
using Spoti_bot.Bot.Data.User;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Bot
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Data.User.User> SaveUser(Message message)
        {
            // Save the user to the storage.
            var user = _mapper.Map<Data.User.User>(message.From);

            return await _userRepository.Upsert(user);
        }
    }
}
