using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Configuration;
using WeaverCore.GodsOfGlory;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace Assets.Scripts
{
#if !UNITY_EDITOR
	class OnSceneLoad
	{
		[OnInit]
		public static void OnInit()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			if (arg0.name.Contains("GG_Workshop"))
			{
				var settings = GlobalWeaverSettings.GetSettings<IKGSettings>();
				if (settings.hardMode)
				{
					GGWorkshop.ChangeStatue("GG_Statue_Grimm", "Absolute Inferno King Grimm");
				}
				else
				{
					GGWorkshop.ChangeStatue("GG_Statue_Grimm", "Inferno King Grimm");
				}
			}
		}
	}
#endif
}
