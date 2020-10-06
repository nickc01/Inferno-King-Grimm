﻿using System;

namespace WeaverCore.Attributes
{
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRuntimeInitAttribute : PriorityAttribute
    {
        public OnRuntimeInitAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
