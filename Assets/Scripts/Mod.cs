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
			Modding.ModHooks.LanguageGetHook += Instance_LanguageGetHook;
		}

		private string Instance_LanguageGetHook(string key, string sheetTitle)
		{
			if (key == "NAME_NIGHTMARE_GRIMM")
			{
				var settings = Panel.GetSettings<IKGSettings>();
				if (settings.hardMode)
				{
					return "Absolute Inferno King Grimm";
				}
				else
				{
					return "Inferno King Grimm";
				}
			}
			else
			{
				return Language.GetStringInternal(key, sheetTitle);
			}
			/*WeaverLog.Log("Language Key = " + key);
			WeaverLog.Log("Sheet Title = " + sheetTitle);
			var value = Language.GetStringInternal(key, sheetTitle);
			WeaverLog.Log("Value = " + value);
			return value;*/
			//throw new NotImplementedException();
		}

		public override string GetVersion()
		{
			return "3.1.0.0";
		}
	}
}

