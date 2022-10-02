using System;

namespace SpotiBot.Library.Exceptions
{
    public class LoginRequestNullException : Exception
    {
        public LoginRequestNullException(string loginRequestId)
        {
            Data[nameof(loginRequestId)] = loginRequestId;
        }
    }
}
