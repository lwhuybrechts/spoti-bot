﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Library
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, ITableEntity, new()
    {
        private readonly CloudTable _cloudTable;
        protected readonly string _defaultPartitionKey;

        public BaseRepository(CloudTable cloudTable, string defaultPartitionKey)
        {
            _cloudTable = cloudTable;
            _defaultPartitionKey = defaultPartitionKey;

            // TODO: do only once? Maybe create a seperate function (or in setup) that creates all tables.
            //_cloudTable.CreateIfNotExists();
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

            // Get the item by it's rowKey.
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            
            var tableResult = await _cloudTable.ExecuteAsync(operation);

            if (tableResult.HttpStatusCode == 404)
                return null;

            return tableResult.Result as T;
        }

        public Task<T> Get(T item)
        {
            return Get(item.RowKey, item.PartitionKey);
        }

        public Task<List<T>> GetAll()
        {
            var query = new TableQuery<T>();
            
            return ExecuteSegmentedQueries(query);
        }

        public Task<List<T>> GetAllByRowKey(string rowKey)
        {
            var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, rowKey);
            var query = new TableQuery<T>().Where(rowKeyFilter);

            return ExecuteSegmentedQueries(query);
        }

        public Task<List<T>> GetAllByPartitionKey(string partitionKey)
        {
            var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<T>().Where(partitionKeyFilter);
            
            return ExecuteSegmentedQueries(query);
        }

        public async Task<List<string>> GetAllRowKeys(string partitionKey = "")
        {
            // Make sure the PartitionKey is set.
            if (string.IsNullOrEmpty(partitionKey))
                partitionKey = _defaultPartitionKey;

            // Create a dynamic query that only selects RowKeys.
            var tableQuery = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, partitionKey))
                .Select(new string[] { nameof(TableEntity.RowKey) });

            // Resolve the query results by returning the RowKey value.
            EntityResolver<string> resolver = (partitionKey, rowKey, timestamp, properties, etag) =>
                properties.ContainsKey(nameof(TableEntity.RowKey)) ? properties[nameof(TableEntity.RowKey)].StringValue : null;

            return await ExecuteDynamicSegmentedQueries(tableQuery, resolver);
        }

        protected async Task<T> GetSingle(TableQuery<T> query)
        {
            return (await ExecuteSegmentedQueries(query)).SingleOrDefault();
        }

        public async Task<T> Upsert(T item)
        {
            AddMissingPartitionKey(item);

            var operation = TableOperation.InsertOrMerge(item);

            return (await _cloudTable.ExecuteAsync(operation)).Result as T;
        }

        public async Task Upsert(List<T> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException($"Cannot upsert items, items list is {(items == null ? "null" : "empty")}.", nameof(items));

            foreach (var item in items)
                AddMissingPartitionKey(item);

            var batches = CreateBatches(items, TableOperationType.InsertOrMerge);

            foreach (var batch in batches)
                await _cloudTable.ExecuteBatchAsync(batch);
        }

        public async Task Delete(T item)
        {
            AddMissingPartitionKey(item);

            var operation = TableOperation.Delete(item);

            await _cloudTable.ExecuteAsync(operation);
        }

        public async Task Delete(List<T> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException($"Cannot delete items, items list is {(items == null ? "null" : "empty")}.", nameof(items));

            var batches = CreateBatches(items, TableOperationType.Delete);

            foreach (var batch in batches)
                await _cloudTable.ExecuteBatchAsync(batch);
        }

        public async Task Truncate()
        {
            // TODO: this seems really ineffecient. Maybe just delete the table?
            var items = await GetAll();

            await Delete(items);
        }

        /// <summary>
        /// Execute a query in multiple parts and return the combined results.
        /// If there are less than 1000 entities only one query is used.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        protected async Task<List<T>> ExecuteSegmentedQueries(TableQuery<T> query)
        {
            var items = new List<T>();

            // Initialize the continuation token to null to start from the beginning of the table.
            TableContinuationToken continuationToken = null;

            do
            {
                // Retrieve a segment (up to 1000 entities).
                var tableQueryResult = await _cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                items.AddRange(tableQueryResult.Results);

                // Assign the new continuation token to tell the service where to continue on the next iteration (or null if it has reached the end).
                continuationToken = tableQueryResult.ContinuationToken;

                // Loop until a null continuation token is received, indicating the end of the table.
            } while (continuationToken != null);

            return items;
        }

        /// <summary>
        /// Execute a dynamic query in multiple parts and return the combined results.
        /// If there are less than 1000 entities only one query is used.
        /// </summary>
        /// <typeparam name="TModel">The type we want to resolve the query results to.</typeparam>
        /// <param name="dynamicTableQuery">The dynamic query to execute.</param>
        /// <param name="resolver">The resolver that maps the query result to <typeparamref name="TModel">TModel</typeparamref>.</param>
        private async Task<List<TModel>> ExecuteDynamicSegmentedQueries<TModel>(TableQuery<DynamicTableEntity> dynamicTableQuery, EntityResolver<TModel> resolver)
        {
            var items = new List<TModel>();

            // Initialize the continuation token to null to start from the beginning of the table.
            TableContinuationToken continuationToken = null;

            do
            {
                // Retrieve a segment (up to 1000 entities).
                var tableQueryResult = await _cloudTable.ExecuteQuerySegmentedAsync(dynamicTableQuery, resolver, continuationToken);

                items.AddRange(tableQueryResult.Results);

                // Assign the new continuation token to tell the service where to continue on the next iteration (or null if it has reached the end).
                continuationToken = tableQueryResult.ContinuationToken;

                // Loop until a null continuation token is received, indicating the end of the table.
            } while (continuationToken != null);

            return items;
        }

        /// <summary>
        /// Create batches of up to 100 operations for all items.
        /// </summary>
        /// <param name="items">The items to create batches for.</param>
        /// <returns>A list of batches that are ready to be executed.</returns>
        private static List<TableBatchOperation> CreateBatches(List<T> items, TableOperationType tableOperationType)
        {
            // Batches support up to 100 operations.
            const int maxBatchSize = 100;

            var batches = new List<TableBatchOperation>();

            var batch = new TableBatchOperation();
            foreach (var item in items)
            {
                // When the max batch size is reached, create a new batch.
                if (items.IndexOf(item) > 0 && items.IndexOf(item) % maxBatchSize == 0)
                {
                    batches.Add(batch);
                    batch = new TableBatchOperation();
                }

                AddTableOperationToBatch(tableOperationType, batch, item);
            }

            batches.Add(batch);
            return batches;
        }

        private static void AddTableOperationToBatch(TableOperationType tableOperationType, TableBatchOperation batch, T item)
        {
            // TODO: check if a function like this is already available in the azure storage package.
            switch (tableOperationType)
            {
                case TableOperationType.InsertOrMerge:
                    batch.Add(TableOperation.InsertOrMerge(item));
                    break;
                case TableOperationType.Delete:
                    batch.Add(TableOperation.Delete(item));
                    break;
                case TableOperationType.Insert:
                    batch.Add(TableOperation.Insert(item));
                    break;
                case TableOperationType.Replace:
                    batch.Add(TableOperation.Replace(item));
                    break;
                case TableOperationType.Merge:
                    batch.Add(TableOperation.Merge(item));
                    break;
                case TableOperationType.InsertOrReplace:
                    batch.Add(TableOperation.InsertOrReplace(item));
                    break;
                case TableOperationType.Retrieve:
                    throw new ArgumentOutOfRangeException(nameof(tableOperationType),
                        $"TableOperationType {tableOperationType} not supported for {nameof(AddTableOperationToBatch)}.");
            }
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