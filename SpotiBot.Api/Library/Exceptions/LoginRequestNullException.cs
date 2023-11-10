using System;

namespace SpotiBot.Api.Library.Exceptions
{
    public class LoginRequestNullException : Exception
    {
        public LoginRequestNullException(string loginRequestId)
        {
            Data[nameof(loginRequestId)] = loginRequestId;
        }
    }
}
