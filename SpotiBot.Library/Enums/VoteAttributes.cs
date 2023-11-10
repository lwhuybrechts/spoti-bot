using System;

namespace SpotiBot.Library.Enums
{
    public class VoteAttributes : Attribute
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public class UseNegativeOperatorAttribute : Attribute
        {

        }
    }
}
