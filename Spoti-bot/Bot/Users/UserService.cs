using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.Votes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IChatMemberRepository _chatMemberRepository;

        public UserService(
            IUserRepository userRepository,
            IVoteRepository voteRepository,
            IChatMemberRepository chatMemberRepository)
        {
            _userRepository = userRepository;
            _voteRepository = voteRepository;
            _chatMemberRepository = chatMemberRepository;
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
        public async Task<List<User>> GetUpvoteUsers(string playlistId, string trackId)
        {
            var votes = await _voteRepository.GetVotes(playlistId, trackId);
            var upvotes = votes.Where(x => x.Type == VoteType.Upvote).ToList();

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
