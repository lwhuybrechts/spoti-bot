using System;

namespace SpotiBot.Library.Exceptions
{
    public class QueryParameterNullException : Exception
    {
        public QueryParameterNullException(string queryParameterName)
        {
            Data[nameof(queryParameterName)] = queryParameterName;
        }
    }
}
