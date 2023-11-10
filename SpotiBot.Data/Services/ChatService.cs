using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Bot;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        public ChatService(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
        }

        public async Task<Chat?> Get(long id)
        {
            var chat = await _chatRepository.Get(id);

            return chat != null
                ? _mapper.Map(chat)
                : null;
        }

        public async Task<Chat> Save(Chat chat)
        {
            return _mapper.Map(await _chatRepository.Upsert(_mapper.Map(chat)));
        }
    }
}
