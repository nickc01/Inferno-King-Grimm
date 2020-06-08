using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesMove : GrimmMove
{
	[Header("Grimm Spike Settings")]
	[SerializeField]
	float spikeWaitTime = 1.35f;

	SpikesController spikeController;

	void Awake()
	{
		spikeController = Instantiate(Prefabs.spikeControllerPrefab, Prefabs.spikeControllerPrefab.transform.position, Quaternion.identity);
	}

	public override IEnumerator DoMove()
	{
		spikeController.Play();
		yield return new WaitForSeconds(spikeWaitTime);

		yield return new WaitForSeconds(0.6f);
	}
}
