using System;
using System.ComponentModel;

namespace Spoti_bot.Library
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the description attribute of an enum value.
        /// </summary>
        /// <param name="enumValue">The enum value to get the description of.</param>
        /// <returns>The value of the description attribute.</returns>
        public static string ToDescriptionString<T>(this T enumValue) where T : Enum
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])enumValue
               .GetType()
               .GetField(enumValue.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0
                ? attributes[0].Description
                : string.Empty;
        }
    }
}
