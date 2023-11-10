using System;

namespace SpotiBot.Api.Library.Exceptions
{
    public class DescriptionAttributeMissingException : Exception
    {
        public DescriptionAttributeMissingException(Enum enumValue)
        {
            Data["enumType"] = enumValue.GetType();
            Data[nameof(enumValue)] = enumValue;
        }
    }
}
