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

		internal static bool TentBossTitleHook(string key, string sheetTitle, string orig, string current, out string res)
		{
			var settings = Panel.GetSettings<IKGSettings>();
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
				return true;
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
				return true;
			}
			res = "";
			return false;
		}

		internal static bool GodhomeLanguageHook(string key, string sheetTitle, string orig, string current, out string res)
		{
			if (key == "NAME_NIGHTMARE_GRIMM")
			{
				var settings = Panel.GetSettings<IKGSettings>();
				string title = "";
				/*if (settings.Infinite)
				{
					title = "Infinite " + Environment.NewLine;
				}*/
				if (settings.hardMode)
				{
					title += "Absolute Inferno King Grimm";
					res = title;
					return true;
				}
				else
				{
					title += "Inferno King Grimm";
					res = title;
					return true;
				}
			}
			else
			{
				res = "";
				return false;
			}
		}

		public override string GetVersion()
		{
			return "4.0.0.0";
		}
	}
}

