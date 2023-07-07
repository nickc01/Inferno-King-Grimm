using System.Collections;
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
				yield return new WaitForSeconds(0.65f / InfernoKingGrimm.GetInfiniteSpeed(0.5f,5f));
			}
			else
			{
				yield return spikeController.DoRegularAsync(0.70f);
				yield return new WaitForSeconds(1f / InfernoKingGrimm.GetInfiniteSpeed(0.5f, 5f));
			}
		}
		else
		{
			if (Grimm.Settings.hardMode)
			{
				if (Grimm.BossStage == 1)
				{
					yield return spikeController.PlayAlternatingTripleAsync();
					yield return new WaitForSeconds(0.6f / InfernoKingGrimm.GetInfiniteSpeed(1f,8f));
				}
				else if (Grimm.BossStage == 2)
				{
					yield return spikeController.PlayDashSpikes(maxDashDistance, 3);
					yield return new WaitForSeconds(0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,8f));
				}
				else if (Grimm.BossStage == 3)
				{
					yield return spikeController.PlayDashSpikes(maxDashDistance + 1, 2);
					yield return new WaitForSeconds(0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,8f));
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

                if (Grimm.FightingInPantheon)
                {
                    yield return new WaitForSeconds(0.15f / InfernoKingGrimm.InfiniteSpeed);
                }
                else if (Grimm.BossStage >= 3)
				{
					yield return new WaitForSeconds(0.3f / InfernoKingGrimm.GetInfiniteSpeed(1f,8f));
				}
				else
				{
					yield return new WaitForSeconds(0.6f / InfernoKingGrimm.GetInfiniteSpeed(1f,8f));
				}
			}
		}
		doingSpikes = false;
	}

	public override IEnumerator DoMove()
	{
		var balloonMove = GetComponent<BalloonMove>();
		if ((InfernoKingGrimm.GetInfiniteSpeed(1f,5f) >= 2f && !Grimm.Invisible) || Grimm.FightingInPantheon)
		{
			yield return Grimm.TeleportIn(balloonMove.BalloonPosition - new Vector2(0f,50f));
		}
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
