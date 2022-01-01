using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;

public class GrimmRealBat : EntityHealth
{
	[NonSerialized]
	public InfernoKingGrimm Grimm;

	public override bool Hit(HitInfo hit)
	{
		Health = int.MaxValue - 1;
		var oldHealth = Health;
		var result = base.Hit(hit);
		var newHealth = Health;

        /*switch (Grimm.Health.HealthDirection)
		{
			case HealthDirection.Down:
				Grimm.GrimmHealth.Health -= (oldHealth - newHealth);
				break;
			case HealthDirection.Up:
				Grimm.GrimmHealth.Health += (oldHealth - newHealth);
				break;
		}*/
        if (Grimm.Health.HasModifier<InfiniteHealthModifier>())
        {
			Grimm.GrimmHealth.Health += (oldHealth - newHealth);
		}
		else
        {
			Grimm.GrimmHealth.Health -= (oldHealth - newHealth);
		}

		return result;


		/*var grimmHealth = Grimm.GetComponent<EntityHealth>();
		if (grimmHealth.HealthDirection == HealthDirection.Up)
		{
			return grimmHealth.Hit(hit);
		}
		else
		{
			return base.Hit(hit);
		}*/
	}
}
