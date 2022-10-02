using SpotiBot.Bot.Chats;
using System.Threading.Tasks;

namespace SpotiBot.Bot.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatMemberRepository _chatMemberRepository;

        public UserService(
            IUserRepository userRepository,
            IChatMemberRepository chatMemberRepository)
        {
            _userRepository = userRepository;
            _chatMemberRepository = chatMemberRepository;
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
    }
}
