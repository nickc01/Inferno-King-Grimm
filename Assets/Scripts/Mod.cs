using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;


namespace IKG
{
	public class InfernoGrimmMod : WeaverMod
	{
		public override void Initialize()
		{
			base.Initialize();
			try
			{
				var shader = Shader.Find("Sprites/GrimmShaderV2");
				WeaverLog.Log("MOD SHADER = " + shader);
			}
			catch (Exception e)
			{
				WeaverLog.LogError("IKG E => " + e);
			}
		}


		public override string GetVersion()
		{
			return "1.1.0.0";
		}
	}
}

