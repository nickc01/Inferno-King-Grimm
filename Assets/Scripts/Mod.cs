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
			Modding.ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
			//Modding.ModHooks.LanguageGetHook += Instance_LanguageGetHook;
		}

		private bool ModHooks_LanguageGetHook(string key, string sheetTitle, string orig, string current, out string res)
		{
			if (key == "NAME_NIGHTMARE_GRIMM")
			{
				var settings = Panel.GetSettings<IKGSettings>();
				if (settings.hardMode)
				{
					res = "Absolute Inferno King Grimm";
					return true;
				}
				else
				{
					res = "Inferno King Grimm";
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
			return "3.1.0.0";
		}
	}
}

