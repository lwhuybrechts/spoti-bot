using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Spotify;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class TrackService : ITrackService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly IMapper _mapper;

        public TrackService(ITrackRepository trackRepository, IMapper mapper)
        {
            _trackRepository = trackRepository;
            _mapper = mapper;
        }

        public async Task<Track?> Get(string trackId, string playlistId)
        {
            var track = await _trackRepository.Get(trackId, playlistId);

            return track != null
                ? _mapper.Map(track)
                : null;
        }

        public async Task<List<Track>> GetByPlaylist(string playlistId)
        {
            return _mapper.Map(await _trackRepository.GetAllByPartitionKey(playlistId));
        }

        public async Task<Track> Upsert(Track track)
        {
            return _mapper.Map(await _trackRepository.Upsert(_mapper.Map(track)));
        }

        public Task Upsert(List<Track> tracks)
        {
            return _trackRepository.Upsert(_mapper.Map(tracks));
        }

        public Task Delete(List<Track> tracks)
        {
            return _trackRepository.Delete(_mapper.Map(tracks));
        }
    }
}
