using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatMemberRepository _chatMemberRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IChatMemberRepository chatMemberRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _chatMemberRepository = chatMemberRepository;
            _mapper = mapper;
        }

        public async Task<User?> Get(long id)
        {
            var model = await _userRepository.Get(id);

            return model != null
                ? _mapper.Map(model)
                : null;
        }

        public async Task<List<User>> Get(List<long> ids)
        {
            // TODO: check the voterepository, this might be possible.

            // Fetch all users (since Azure table storage does not support WHERE IN queries).
            var userModels = await _userRepository.GetAll();

            // Only return users that voted.
            return _mapper.Map(userModels.Where(x => ids.Contains(x.Id)).ToList());
        }

        public async Task<List<User>> GetAllExcept(long id)
        {
            var userModels = await _userRepository.GetAll();

            return _mapper.Map(userModels.Where(x => x.Id != id).ToList());
        }

        public async Task<User> SaveUser(User user, long chatId)
        {
            // Save the user to the storage.
            var savedUser = _mapper.Map(await _userRepository.Upsert(_mapper.Map(user)));

            // Save the user as a ChatMember.
            await _chatMemberRepository.Upsert(new Models.ChatMember
            {
                ChatId = chatId,
                UserId = savedUser.Id
            });

            return savedUser;
        }
    }
}
