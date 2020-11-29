using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Library
{
    public interface IBaseRepository<T> where T : class, ITableEntity, new()
    {
        Task<T> Get(string rowKey, string partitionKey = "");
        Task<T> Get(T item);
        Task<List<T>> GetAll();
        Task<List<T>> GetPartition(string partitionKey);
        Task<List<string>> GetAllRowKeys(string partitionKey = "");
        Task<T> Upsert(T item);
        Task Upsert(List<T> items);
        Task Delete(T item);
        Task Delete(List<T> items);
        Task Truncate();
    }
}