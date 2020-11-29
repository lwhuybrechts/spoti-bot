using AutoMapper;
using Spoti_bot.Bot.Upvotes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUpvoteRepository _upvoteRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IUpvoteRepository upvoteRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _upvoteRepository = upvoteRepository;
            _mapper = mapper;
        }

        public async Task<User> SaveUser(Telegram.Bot.Types.User telegramUser)
        {
            // Save the user to the storage.
            var user = _mapper.Map<User>(telegramUser);

            return await _userRepository.Upsert(user);
        }

        /// <summary>
        /// Get the users that upvoted a track.
        /// </summary>
        public async Task<List<User>> GetUpvoteUsers(string trackId)
        {
            var upvotes = await _upvoteRepository.GetPartition(trackId);

            if (!upvotes.Any())
                return new List<User>();

            var users = await _userRepository.GetAll();

            var upvoteUsers = users
                .Where(x => upvotes
                    .Select(x => x.UserId.ToString())
                    .Contains(x.Id)
                ).ToList();

            return upvoteUsers;
        }
    }
}
