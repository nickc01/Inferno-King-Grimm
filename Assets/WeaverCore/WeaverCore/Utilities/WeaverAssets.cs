﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	public static class WeaverAssets
	{
		public static readonly string WeaverAssetBundleName = "weavercore_bundle";

		static WeaverAssets_I Impl;


		[Serializable]
		public class WeaverAssetsException : Exception
		{
			public WeaverAssetsException() { }
			public WeaverAssetsException(string message) : base(message) { }
			public WeaverAssetsException(string message, Exception inner) : base(message, inner) { }
			protected WeaverAssetsException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		[OnInit(int.MinValue / 2)]
		static void OnInit()
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAssets_I>();
				Impl.Initialize();
			}
		}

		public static T LoadWeaverAsset<T>(string name) where T : UnityEngine.Object
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAssets_I>();
				Impl.Initialize();
			}

			return Impl.LoadAsset<T>(name);
		}
	}
}
