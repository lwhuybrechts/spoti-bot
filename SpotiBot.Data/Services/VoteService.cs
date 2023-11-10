using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IMapper _mapper;

        public VoteService(IVoteRepository voteRepository, IMapper mapper)
        {
            _voteRepository = voteRepository;
            _mapper = mapper;
        }

        public async Task<Vote?> Get(Vote vote)
        {
            var voteModel = await _voteRepository.Get(_mapper.Map(vote));

            return voteModel != null
                ? _mapper.Map(voteModel)
                : null;
        }

        public async Task<List<Vote>> Get(string playlistId, string trackId)
        {
            return _mapper.Map(await _voteRepository.GetVotes(playlistId, trackId));
        }

        public async Task<List<Vote>> Get(string playlistId)
        {
            return _mapper.Map(await _voteRepository.GetAllByPartitionKey(playlistId));
        }

        public async Task<Vote> Upsert(Vote vote)
        {
            return _mapper.Map(await _voteRepository.Upsert(_mapper.Map(vote)));
        }

        public Task Delete(Vote vote)
        {
            return _voteRepository.Delete(_mapper.Map(vote));
        }
    }
}
