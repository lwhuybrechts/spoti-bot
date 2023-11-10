using System;

namespace SpotiBot.Api.Library.Exceptions
{
    public class ChatAdminNullException : Exception
    {
        public ChatAdminNullException(long chatId, long userId)
            : base($"Admin user with id {userId} could not be found for chat {chatId}.")
        {

        }
    }
}
