using System;

namespace Spoti_bot.Library.Exceptions
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
