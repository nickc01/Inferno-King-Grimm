using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Components;


public class GrimmHealth : EntityHealth
{
	protected override void OnHealthUpdate(int oldHealth, int newHealth)
	{
		if (InfernoKingGrimm.GodMode)
		{
			if (newHealth < oldHealth)
			{
				for (int i = InfernoKingGrimm.GrimmsFighting.Count - 1; i >= 0; i--)
				{
					if (i < InfernoKingGrimm.GrimmsFighting.Count)
					{
						var grimm = InfernoKingGrimm.GrimmsFighting[i];
						if (grimm.EntityHealth.Health != newHealth)
						{
							grimm.EntityHealth.Health = newHealth;
						}
					}
				}
				/*foreach (var grimm in InfernoKingGrimm.GrimmsFighting)
				{
					if (grimm.EntityHealth.Health != newHealth)
					{
						grimm.EntityHealth.Health = newHealth;
					}
				}*/
			}
		}
	}
}

