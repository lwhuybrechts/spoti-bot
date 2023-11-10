using SpotiBot.Library.BusinessModels.Bot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface IVoteService
    {
        Task<Vote?> Get(Vote vote);
        Task<List<Vote>> Get(string playlistId, string trackId);
        Task<List<Vote>> Get(string playlistId);
        Task<Vote> Upsert(Vote vote);
        Task Delete(Vote vote);
    }
}