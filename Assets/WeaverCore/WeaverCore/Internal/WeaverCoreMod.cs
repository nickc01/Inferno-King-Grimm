﻿using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
    /// <summary>
    /// The mod class for WeaverCore
    /// </summary>
    public sealed class WeaverCore : WeaverMod
    {
        public override void Initialize()
        {
            base.Initialize();
        }


        public override string GetVersion()
        {
            return "0.2.1.0 Beta";
        }
    }
}
