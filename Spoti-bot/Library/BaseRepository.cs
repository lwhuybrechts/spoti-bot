using Azure;
using Azure.Data.Tables;
using SpotiBot.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SpotiBot.Library
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;
        protected readonly string _defaultPartitionKey;

        public BaseRepository(TableClient tableClient, string defaultPartitionKey)
        {
            _tableClient = tableClient;
            _defaultPartitionKey = defaultPartitionKey;

            // TODO: do only once? Maybe create a seperate function (or in setup) that creates all tables.
            //_tableClient.CreateIfNotExists();
        }

        public Task<T> Get(long rowKey, string partitionKey = "")
        {
            return Get(rowKey.ToString(), partitionKey);
        }

        public async Task<T> Get(string rowKey, string partitionKey = "")
        {
            if (string.IsNullOrEmpty(rowKey))
                return null;

            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            try
            {
                // Get the item by it's rowKey.
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response?.Value;
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
                return null;
            }
        }

        public Task<T> Get(T item)
        {
            return Get(item.RowKey, item.PartitionKey);
        }

        public Task<List<T>> GetAll()
        {
            return QueryPageable(x => true);
        }

        public Task<List<T>> GetAllByRowKey(string rowKey)
        {
            return QueryPageable(x => x.RowKey == rowKey);
        }

        public Task<List<T>> GetAllByPartitionKey(string partitionKey)
        {
            return QueryPageable(x => x.PartitionKey == partitionKey);
        }

        public async Task<List<string>> GetAllRowKeys(string partitionKey = "")
        {
            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            return (await QueryPageable(x => x.PartitionKey == partitionKey)).Select(x => x.RowKey).ToList();
        }

        protected async Task<T> GetSingle(Expression<Func<T, bool>> filter)
        {
            return (await QueryPageable(filter)).SingleOrDefault();
        }

        public async Task Upsert(T item)
        {
            AddMissingPartitionKey(item);

            var response = await _tableClient.UpsertEntityAsync(item);

            if (response.IsError)
                throw new UpsertFailedException<T>(item, response);
        }

        public async Task Upsert(List<T> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException($"Cannot upsert items, items list is {(items == null ? "null" : "empty")}.", nameof(items));

            foreach (var item in items)
                AddMissingPartitionKey(item);

            var transactionResponse = await _tableClient.SubmitTransactionAsync(items.Select(x => new TableTransactionAction(TableTransactionActionType.UpsertMerge, x)));

            foreach (var response in transactionResponse.Value)
                if (response.IsError)
                {
                    var index = transactionResponse.Value.ToList().IndexOf(response);

                    throw new UpsertFailedException<T>(items[index], response);
                }
        }

        public async Task Delete(T item)
        {
            AddMissingPartitionKey(item);

            await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey);
        }

        public async Task Delete(List<T> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException($"Cannot delete items, items list is {(items == null ? "null" : "empty")}.", nameof(items));

            var transactionResponse = await _tableClient.SubmitTransactionAsync(items.Select(x => new TableTransactionAction(TableTransactionActionType.Delete, x)));

            foreach (var response in transactionResponse.Value)
                if (response.IsError)
                {
                    var index = transactionResponse.Value.ToList().IndexOf(response);

                    throw new DeleteFailedException<T>(items[index], response);
                }
        }

        public async Task Truncate()
        {
            // TODO: this seems really ineffecient. Maybe just delete the table?
            var items = await GetAll();

            await Delete(items);
        }

        protected async Task<List<T>> QueryPageable(Expression<Func<T, bool>> filter)
        {
            var items = new List<T>();

            // Initialize the continuation token to null to start from the beginning of the table.
            string continuationToken = null;

            await foreach (var page in _tableClient.QueryAsync(filter).AsPages(continuationToken))
            {
                continuationToken = page.ContinuationToken;
                items.AddRange(page.Values);

                if (string.IsNullOrEmpty(continuationToken) || page.Values.Count == 0)
                    return items;
            }

            return items;
        }

        /// <summary>
        /// To Upsert or Delete an item it needs to have a PartitionKey.
        /// If it's missing, add the default PartitionKey for it's type.
        /// </summary>
        private void AddMissingPartitionKey(T item)
        {
            if (string.IsNullOrEmpty(item.PartitionKey))
                item.PartitionKey = _defaultPartitionKey;
        }
    }
}