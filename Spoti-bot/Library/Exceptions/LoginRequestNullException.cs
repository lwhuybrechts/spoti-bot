using System;

namespace Spoti_bot.Library.Exceptions
{
    public class LoginRequestNullException : Exception
    {
        public LoginRequestNullException(string loginRequestId)
        {
            Data[nameof(loginRequestId)] = loginRequestId;
        }
    }
}
