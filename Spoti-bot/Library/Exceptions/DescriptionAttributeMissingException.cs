using System;

namespace SpotiBot.Library.Exceptions
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
