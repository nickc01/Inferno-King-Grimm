using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;

public class GrimmHealthManager : HealthManager
{
	ReignitedKingGrimm grimm;

	[SerializeField]
	int bossHealthStages = 4;

	int nextHealthMilestone;
	int currentHealthStage = 0;

	public int BossStages
	{
		get
		{
			return bossHealthStages;
		}
	}

	void Start()
	{
		grimm = GetComponent<ReignitedKingGrimm>();
		nextHealthMilestone = Health - (Health / bossHealthStages);
	}

	public override bool Hit(HitInfo hit)
	{
		grimm.ReachedHealthStage(0);
		return base.Hit(hit);

		int previousHealth = Health;
		var success = base.Hit(hit);
		if (success && Health <= nextHealthMilestone && Health > 0)
		{
			currentHealthStage++;
			grimm.ReachedHealthStage(currentHealthStage);
			nextHealthMilestone = Health - (Health / bossHealthStages);
		}

		return success;
	}

	protected override void OnDeath()
	{
		grimm.OnDeath();
		base.OnDeath();
	}
}
