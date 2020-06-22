using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;

public class GrimmHealthManager : HealthManager
{
	class Milestone
	{
		public int HealthNumber;
		public Action MilestoneReached;

		public Milestone(int health, Action action)
		{
			HealthNumber = health;
			MilestoneReached = action;
		}

		class ComparerDef : IComparer<Milestone>
		{
			public int Compare(Milestone x, Milestone y)
			{
				return x.HealthNumber - y.HealthNumber;
			}
		}

		public static IComparer<Milestone> Comparer = new ComparerDef();
	}


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
		//grimm.ReachedHealthStage(0);
		//return base.Hit(hit);

		/*int previousHealth = Health;
		var success = base.Hit(hit);
		if (success && Health <= nextHealthMilestone && Health > 0)
		{
			currentHealthStage++;
			grimm.ReachedHealthStage(currentHealthStage);
			nextHealthMilestone = Health - (Health / bossHealthStages);
		}

		return success;*/
	}

	protected override void OnDeath()
	{
		grimm.OnDeath();
		base.OnDeath();
	}
}
