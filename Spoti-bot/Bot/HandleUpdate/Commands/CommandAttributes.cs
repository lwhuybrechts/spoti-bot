using System;

namespace Spoti_bot.Bot.HandleUpdate.Commands
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresPrivateChatAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresChatAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresNoChatAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresPlaylistAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresNoPlaylistAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresChatAdminAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RequiresQueryAttribute : Attribute
    { }
}
