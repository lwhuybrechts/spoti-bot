using SpotiBot.Data.Services;
using System;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.SyncHistory
{
    public class SyncHistoryService : ISyncHistoryService
    {
        private readonly IParseHistoryJsonService _parseHistoryJsonService;
        private readonly ITrackService _trackService;
        //private readonly IMapper _mapper;

        public SyncHistoryService(IParseHistoryJsonService parseHistoryJsonService, ITrackService trackRepository)
        {
            _parseHistoryJsonService = parseHistoryJsonService;
            _trackService = trackRepository;
        }

        /// <summary>
        /// Read an exported telegram chat json file and update all found tracks that are also in storage.
        /// </summary>
        public Task<int> SyncTracksFromJson(string jsonString)
        {
            // TODO: update this to support multiple chats. Read the chatId from the json.
            throw new NotImplementedException();

            // Get the tracks from the chat history json.
            //var tracksFromJson = await _parseHistoryJsonService.ParseTracks(jsonString, DateTimeKind.Local);

            //if (tracksFromJson.Count == 0)
            //    return 0;

            //// Only use the first time that tracks where posted.
            //var firstPostedTracksFromJson = tracksFromJson
            //    .GroupBy(x => x.TrackId)
            //    .Select(x => x.First())
            //    .ToList();

            //// Get all the tracks from the storage.
            //var tracksFromStorage = await _trackService.GetAll();

            //// TODO: check TrackState here, add missing tracks and ignore removed tracks.

            //// Only update tracks that are already in our storage.
            //var tracksToSave = firstPostedTracksFromJson
            //    .Where(x => tracksFromStorage
            //        .Any(track => track.Id == x.TrackId)
            //    ).ToList();

            //// Save the tracks to storage.
            //await _trackService.Upsert(_mapper.Map(tracksToSave));

            //return tracksToSave.Count;
        }
    }
}
