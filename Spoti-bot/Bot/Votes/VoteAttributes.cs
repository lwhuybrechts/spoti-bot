﻿using System;

namespace SpotiBot.Bot.Votes
{
    public class VoteAttributes : Attribute
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public class UseNegativeOperatorAttribute : Attribute
        {

        }
    }
}
