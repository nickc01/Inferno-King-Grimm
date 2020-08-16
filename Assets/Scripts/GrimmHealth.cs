using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;

/*public class GrimmHealth : EntityHealth
{


	InfernoKingGrimm grimm;

	[SerializeField]
	int bossHealthStages = 4;

	//int nextHealthMilestone;
	//int currentHealthStage = 0;
	List<Milestone> milestones = new List<Milestone>();

	Milestone nextMilestone;

	public int BossStages
	{
		get
		{
			return bossHealthStages;
		}
	}

	public void AddHealthMilestone(int health, Action whenMilestoneReached)
	{
		if (health >= Health)
		{
			whenMilestoneReached();
		}
		else
		{
			milestones.Add(new Milestone(health, whenMilestoneReached));
			milestones.Sort(Milestone.Comparer);
			nextMilestone = milestones[milestones.Count - 1];
		}
	}

	void Start()
	{
		grimm = GetComponent<InfernoKingGrimm>();
		//nextHealthMilestone = Health - (Health / bossHealthStages);
	}

	public override bool Hit(HitInfo hit)
	{
		if (nextMilestone == null)
		{
			return base.Hit(hit);
		}
		else
		{
			var successfulHit = base.Hit(hit);
			if (successfulHit)
			{
				while (nextMilestone != null && nextMilestone.HealthNumber >= Health)
				{
					nextMilestone.MilestoneReached();
					milestones.Remove(nextMilestone);
					if (milestones.Count > 0)
					{
						nextMilestone = milestones[milestones.Count - 1];
					}
					else
					{
						nextMilestone = null;
					}
				}
			}
			return successfulHit;
		}
	}

	protected override void OnDeath()
	{
		grimm.OnDeath();
		base.OnDeath();
	}
}*/
