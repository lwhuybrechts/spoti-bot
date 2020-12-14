using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncHistory
{
    public class SyncHistoryService : ISyncHistoryService
    {
        private readonly IParseHistoryJsonService _parseHistoryJsonService;
        private readonly ITrackRepository _trackRepository;
        private readonly IMapper _mapper;

        public SyncHistoryService(IParseHistoryJsonService parseHistoryJsonService, ITrackRepository trackRepository, IMapper mapper)
        {
            _parseHistoryJsonService = parseHistoryJsonService;
            _trackRepository = trackRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Read an exported telegram chat json file and update all found tracks that are also in storage.
        /// </summary>
        public async Task<int> SyncTracksFromJson(string jsonString)
        {
            // TODO: update this to support multiple chats. Read the chatId from the json.
            throw new NotImplementedException();

            // Get the tracks from the chat history json.
            var tracksFromJson = await _parseHistoryJsonService.ParseTracks(jsonString, DateTimeKind.Local);

            if (tracksFromJson.Count == 0)
                return 0;

            // Only use the first time that tracks where posted.
            var firstPostedTracksFromJson = tracksFromJson
                .GroupBy(x => x.TrackId)
                .Select(x => x.First())
                .ToList();

            // Get all the tracks from the storage.
            var tracksFromStorage = await _trackRepository.GetAll();

            // Only update tracks that are already in our storage.
            var tracksToSave = firstPostedTracksFromJson
                .Where(x => tracksFromStorage
                    .Select(ts => ts.Id)
                    .Contains(x.TrackId)
                ).ToList();

            // Save the tracks to storage.
            await _trackRepository.Upsert(_mapper.Map<List<Track>>(tracksToSave));

            return tracksToSave.Count;
        }
    }
}
