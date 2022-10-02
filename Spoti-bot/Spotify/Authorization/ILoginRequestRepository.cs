using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Authorization
{
    public interface ILoginRequestRepository
    {
        Task<LoginRequest> Get(string rowKey, string partitionKey = "");
        Task<LoginRequest> Get(LoginRequest item);
        Task<List<LoginRequest>> GetAll();
        Task<List<LoginRequest>> GetAllExpired();
        Task<LoginRequest> Upsert(LoginRequest item);
        Task Delete(LoginRequest item);
        Task Delete(List<LoginRequest> items);
    }
}