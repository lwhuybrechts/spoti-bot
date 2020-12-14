using System;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    public static class CommandExtensions
    {
        public static bool RequiresChat(this Command command)
        {
            return HasAttribute<RequiresChatAttribute>(command);
        }

        public static bool RequiresNoChat(this Command command)
        {
            return HasAttribute<RequiresNoChatAttribute>(command);
        }

        public static bool RequiresPlaylist(this Command command)
        {
            return HasAttribute<RequiresPlaylistAttribute>(command);
        }

        public static bool RequiresNoPlaylist(this Command command)
        {
            return HasAttribute<RequiresNoPlaylistAttribute>(command);
        }

        public static bool RequiresChatAdmin(this Command command)
        {
            return HasAttribute<RequiresChatAdminAttribute>(command);
        }

        public static bool RequiresQuery(this Command command)
        {
            return HasAttribute<RequiresQueryAttribute>(command);
        }

        private static bool HasAttribute<T>(Command enumValue) where T : Attribute
        {
            var attributes = (T[])enumValue
                .GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(T), false);

            if (attributes.Length >= 1)
                return true;

            return false;
        }
    }
}
