using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Settings;

namespace IKG
{
	public class InfernoGrimmMod : WeaverMod
	{
		public InfernoGrimmMod() : base("Inferno King Grimm") { }


		public override void Initialize()
		{
			base.Initialize();
			Modding.ModHooks.LanguageGetHook += GodhomeLanguageHook;
		}

		//internal static bool HooksAdded = false;

		/*internal static void AddHooks()
		{
			if (!HooksAdded)
			{
				Modding.ModHooks.LanguageGetHook += TentBossTitleHook;
			}
			HooksAdded = true;
		}*/

		internal static string TentBossTitleHook(string key, string sheetTitle, string res)
		{
			var settings = GlobalSettings.GetSettings<IKGSettings>();
			if (key == "NIGHTMARE_GRIMM_SUPER")
			{
				if (settings.Infinite)
				{
					res = "Inferno";
					if (settings.hardMode)
					{
						res = "Absolute " + res;
					}
					res = "Infinite " + res;
				}
				else
				{
					res = "Inferno King";
					if (settings.hardMode)
					{
						res = "Absolute " + res;
					}
					
				}
				return res;
			}
			else if (key == "NIGHTMARE_GRIMM_MAIN")
			{
				if (settings.Infinite)
				{
					res = "King Grimm";
				}
				else
				{
					res = "Grimm";
				}
				return res;
			}
			return res;
		}

		internal static string GodhomeLanguageHook(string key, string sheetTitle, string res)
		{
			if (key == "NAME_NIGHTMARE_GRIMM")
			{
				var settings = GlobalSettings.GetSettings<IKGSettings>();
				string title = "";
				/*if (settings.Infinite)
				{
					title = "Infinite " + Environment.NewLine;
				}*/
				if (settings.hardMode)
				{
					title += "Absolute Inferno King Grimm";
					return title;
				}
				else
				{
					title += "Inferno King Grimm";
					return title;
				}
			}
			else
			{
				return res;
			}
		}

		public override string GetVersion()
		{
			return "4.2.0.1";
		}
	}
}

