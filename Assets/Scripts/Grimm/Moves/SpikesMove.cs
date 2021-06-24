﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesMove : GrimmMove
{
	[SerializeField]
	float maxDashDistance = 6f;
	//[Header("Grimm Spike Settings")]
	//[SerializeField]
	//float spikeWaitTime = 1.35f;

	SpikesController spikeController;

	void Awake()
	{
		spikeController = Instantiate(Prefabs.spikeControllerPrefab, Prefabs.spikeControllerPrefab.transform.position, Quaternion.identity);
	}

	bool doingSpikes = false;

	IEnumerator SpikeRoutine()
	{
		if (InfernoKingGrimm.GodMode)
		{
			if (Grimm.Settings.hardMode)
			{
				yield return spikeController.DoRegularAsync(0.55f);
				yield return new WaitForSeconds(0.65f / InfernoKingGrimm.InfiniteSpeed);
			}
			else
			{
				yield return spikeController.DoRegularAsync(0.70f);
				yield return new WaitForSeconds(1f / InfernoKingGrimm.InfiniteSpeed);
			}
		}
		else
		{
			if (Grimm.Settings.hardMode)
			{
				if (Grimm.BossStage == 1)
				{
					yield return spikeController.PlayAlternatingTripleAsync();
					yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
				}
				else if (Grimm.BossStage == 2)
				{
					yield return spikeController.PlayDashSpikes(maxDashDistance, 3);
					yield return new WaitForSeconds(0.45f / InfernoKingGrimm.InfiniteSpeed);
				}
				else if (Grimm.BossStage == 3)
				{
					yield return spikeController.PlayDashSpikes(maxDashDistance + 1, 2);
					yield return new WaitForSeconds(0.45f / InfernoKingGrimm.InfiniteSpeed);
				}
			}
			else
			{
				if (Grimm.BossStage == 1)
				{
					//spikeController.Play();
					yield return spikeController.PlayAlternatingAsync();
				}
				else if (Grimm.BossStage == 2)
				{
					if (Grimm.Settings.Infinite)
					{
						yield return spikeController.PlayAlternatingAsync();
					}
					else
					{
						yield return spikeController.PlayMiddleSideAsync();
					}
				}
				else
				{
					yield return spikeController.PlayAlternatingTripleAsync();
				}

				yield return new WaitForSeconds(0.2f);
				//yield return null;

				//yield return new WaitForSeconds(spikeController.CurrentWaitTime);

				if (Grimm.BossStage >= 3)
				{
					yield return new WaitForSeconds(0.3f / InfernoKingGrimm.InfiniteSpeed);
				}
				else
				{
					yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
				}
			}
		}
		doingSpikes = false;
	}

	public override IEnumerator DoMove()
	{
		doingSpikes = true;
		StartCoroutine(SpikeRoutine());
		yield return new WaitUntil(() => !doingSpikes);
	}

	public override void OnStun()
	{
		doingSpikes = false;
		base.OnStun();
	}
}
