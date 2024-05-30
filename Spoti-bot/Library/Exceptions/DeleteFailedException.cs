using Azure;
using Azure.Data.Tables;
using System;
using System.Text.Json;

namespace SpotiBot.Library.Exceptions
{
    public class DeleteFailedException<T> : Exception where T : ITableEntity
    {
        public DeleteFailedException(T item, Response response)
        {
            Data[nameof(item)] = JsonSerializer.Serialize(item);
            Data[nameof(response)] = response.ToString();
        }
    }
}
