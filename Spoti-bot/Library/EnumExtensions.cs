using Spoti_bot.Library.Exceptions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Spoti_bot.Library
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the description attribute of an enum value.
        /// </summary>
        /// <param name="enumValue">The enum value to get the description of.</param>
        /// <exception cref="DescriptionAttributeMissingException">Thrown when the description attribute is missing.</exception>
        /// <returns>The value of the description attribute.</returns>
        public static string ToDescriptionString<T>(this T enumValue) where T : Enum
        {
            var attributes = (DescriptionAttribute[])enumValue
               .GetType()
               .GetField(enumValue.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length == 0)
                throw new DescriptionAttributeMissingException(enumValue);

            return attributes.Single().Description;
        }
    }
}
