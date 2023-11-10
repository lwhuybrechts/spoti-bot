using System;

namespace SpotiBot.Api.Library.Exceptions
{
    public class QueryParameterNullException : Exception
    {
        public QueryParameterNullException(string queryParameterName)
        {
            Data[nameof(queryParameterName)] = queryParameterName;
        }
    }
}
