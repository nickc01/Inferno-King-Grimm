using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class InfernoGrimmMod : WeaverMod
{
	public override bool Unloadable
	{
		get
		{
			return false;
		}
	}

	public override string Version
	{
		get
		{
			return "1.0.0.0";
		}
	}
}

