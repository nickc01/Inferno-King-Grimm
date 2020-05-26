using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebatFirePillar : MonoBehaviour 
{
	public static FirebatFirePillar Spawn(ReignitedKingGrimm grimm, bool autoPlay = true)
	{
		var fireBatPillar = GameObject.Instantiate(ReignitedKingGrimm.Prefabs.FirebatPillarPrefab,grimm.transform,false).GetComponent<FirebatFirePillar>();

		fireBatPillar.gameObject.SetActive(false);

		if (grimm.FaceDirection == Direction.Left)
		{
			fireBatPillar.transform.localPosition = new Vector3(-fireBatPillar.transform.localPosition.x, fireBatPillar.transform.localPosition.y, fireBatPillar.transform.localPosition.z);
		}

		if (autoPlay)
		{
			fireBatPillar.Play();
		}
		return fireBatPillar;
	}


	public void Play()
	{
		gameObject.SetActive(true);
		foreach (var particles in GetComponentsInChildren<ParticleSystem>())
		{
			particles.Play();
		}
		Destroy(gameObject, 10.0f);
	}

	public void Stop()
	{
		gameObject.SetActive(false);
	}
}
