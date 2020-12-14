using System;

namespace Spoti_bot.Library.Exceptions
{
    public class QueryParameterNullException : Exception
    {
        public QueryParameterNullException(string queryParameterName)
        {
            Data[nameof(queryParameterName)] = queryParameterName;
        }
    }
}
