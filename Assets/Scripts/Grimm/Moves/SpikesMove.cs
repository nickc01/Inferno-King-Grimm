using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesMove : GrimmMove
{
	//[Header("Grimm Spike Settings")]
	//[SerializeField]
	//float spikeWaitTime = 1.35f;

	SpikesController spikeController;

	void Awake()
	{
		spikeController = Instantiate(Prefabs.spikeControllerPrefab, Prefabs.spikeControllerPrefab.transform.position, Quaternion.identity);
	}

	public override IEnumerator DoMove()
	{
		if (Grimm.BossStage == 1)
		{
			//spikeController.Play();
			yield return spikeController.PlayAlternatingAsync();
		}
		else if (Grimm.BossStage == 2)
		{
			yield return spikeController.PlayMiddleSideAsync();
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
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}

	}
}
