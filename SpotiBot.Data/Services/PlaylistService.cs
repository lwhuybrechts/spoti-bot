using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IMapper _mapper;

        public PlaylistService(IPlaylistRepository playlistRepository, IMapper mapper)
        {
            _playlistRepository = playlistRepository;
            _mapper = mapper;
        }

        public async Task<Playlist?> Get(string id)
        {
            var model = await _playlistRepository.Get(id);

            return model != null
                ? _mapper.Map(model)
                : null;
        }

        public async Task<Playlist> Save(Playlist playlist)
        {
            return _mapper.Map(await _playlistRepository.Upsert(_mapper.Map(playlist)));
        }
    }
}
