using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;

public class GrimmRealBat : HealthManager
{
	public override bool Hit(HitInfo hit)
	{
		if (IsValidHit(hit) == HitResult.Valid)
		{
			PlayHitEffects(hit);
			return true;
		}
		return false;
	}
}
