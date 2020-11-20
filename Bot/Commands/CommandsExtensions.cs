using System.ComponentModel;

namespace Spoti_bot.Bot.Commands
{
    public static class CommandsExtensions
    {
        /// <summary>
        /// Extension so we can check if a command should be handled.
        /// </summary>
        /// <param name="command">The command to get the expected text message for.</param>
        /// <returns>The text that should be sent to trigger this command.</returns>
        public static string ToDescriptionString(this Command command)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])command
               .GetType()
               .GetField(command.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0
                ? attributes[0].Description
                : string.Empty;
        }
    }
}
