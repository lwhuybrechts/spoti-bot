﻿using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Library
{
    public interface IBaseRepository<T> where T : class, ITableEntity, new()
    {
        Task<T> Get(string rowKey, string partitionKey = "");
        Task<T> Get(long rowKey, string partitionKey = "");
        Task<T> Get(T item);
        Task<List<T>> GetAll();
        Task<List<T>> GetAllByRowKey(string rowKey);
        Task<List<T>> GetAllByPartitionKey(string partitionKey);
        Task<List<string>> GetAllRowKeys(string partitionKey = "");
        Task Upsert(T item);
        Task Upsert(List<T> items);
        Task Delete(T item);
        Task Delete(List<T> items);
        Task Truncate();
    }
}