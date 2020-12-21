using AutoMapper;
using Spoti_bot.Bot.Chats;
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
        private readonly IChatMemberRepository _chatMemberRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IUpvoteRepository upvoteRepository,
            IChatMemberRepository chatMemberRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _upvoteRepository = upvoteRepository;
            _chatMemberRepository = chatMemberRepository;
            _mapper = mapper;
        }

        public Task<User> Get(long id)
        {
            return _userRepository.Get(id);
        }

        public async Task<User> SaveUser(User user, long chatId)
        {
            // Save the user to the storage.
            var savedUser = await _userRepository.Upsert(user);

            // Save the user as a ChatMember.
            await _chatMemberRepository.Upsert(new ChatMember
            {
                ChatId = chatId,
                UserId = savedUser.Id
            });

            return savedUser;
        }

        /// <summary>
        /// Get the users that upvoted a track.
        /// </summary>
        public async Task<List<User>> GetUpvoteUsers(string trackId)
        {
            var upvotes = await _upvoteRepository.GetAllByPartitionKey(trackId);

            if (!upvotes.Any())
                return new List<User>();

            // TODO: only fetch users from a certain partition, or already use query filters.
            var users = await _userRepository.GetAll();

            var upvoteUsers = users
                .Where(x => upvotes
                    .Select(x => x.UserId)
                    .Contains(x.Id)
                ).ToList();

            return upvoteUsers;
        }
    }
}
