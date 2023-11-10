using SpotiBot.Api.Library.Exceptions;
using System;
using System.ComponentModel;
using System.Linq;

namespace SpotiBot.Api.Library.Extensions
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

        /// <summary>
        /// Check if an attribute exists on an enum value.
        /// </summary>
        /// <returns>True if the enum value has the attribute.</returns>
        public static bool HasAttribute<TEnum, TAttribute>(this TEnum enumValue) where TEnum : Enum where TAttribute : Attribute
        {
            var attributes = (TAttribute[])enumValue
                .GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(TAttribute), false);

            if (attributes.Length >= 1)
                return true;

            return false;
        }
    }
}
