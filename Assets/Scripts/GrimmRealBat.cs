using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;

public class GrimmRealBat : EntityHealth
{
	public override bool Hit(HitInfo hit)
	{
		Health = int.MaxValue - 1;
		return base.Hit(hit);
	}
}
