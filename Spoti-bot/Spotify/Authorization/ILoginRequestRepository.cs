using Spoti_bot.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface ILoginRequestRepository : IBaseRepository<LoginRequest>
    {
        Task<List<LoginRequest>> GetAllExpired();
    }
}