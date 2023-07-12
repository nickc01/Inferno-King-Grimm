using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Settings;

public class GrimmHealth : EntityHealth
{
    class Modifier : IHealthModifier
    {
		InfernoKingGrimm grimm;

		public Modifier(InfernoKingGrimm grimm)
        {
			this.grimm = grimm;
        }

		public int Priority => int.MaxValue;

        public int OnHealthChange(int oldHealth, int newHealth)
        {
            var infinite = grimm.Settings.Infinite;
            if (InfernoKingGrimm.GodMode)
			{
                if (newHealth != oldHealth)
				{
                    for (int i = InfernoKingGrimm.GrimmsFighting.Count - 1; i >= 0; i--)
					{
                        if (i < InfernoKingGrimm.GrimmsFighting.Count)
						{
                            var otherGrimm = InfernoKingGrimm.GrimmsFighting[i];
                            if (otherGrimm.HealthComponent.Health != newHealth && otherGrimm != grimm)
							{
                                otherGrimm.HealthComponent.Health = newHealth;
							}
                        }
                    }
                    if (!infinite && GeoCounter.Instance != null && GeoCounter.Instance.GeoText != null)
					{
                        GeoCounter.Instance.GeoText = (grimm.MaxHealth - newHealth).ToString();
                    }
                }
            }
            if (infinite)
			{
                InfernoKingGrimm.InfiniteSpeed = 1f + (newHealth / InfernoKingGrimm.DoublingRatio);
                if (GeoCounter.Instance != null && GeoCounter.Instance.GeoText != null)
                {
                    GeoCounter.Instance.GeoText = (newHealth - 1).ToString();
                }
            }
            return newHealth;
		}
    }

    InfernoKingGrimm grimm;

	protected override void Awake()
	{
		grimm = GetComponent<InfernoKingGrimm>();
		AddModifier(new Modifier(grimm));
		base.Awake();
	}
}

