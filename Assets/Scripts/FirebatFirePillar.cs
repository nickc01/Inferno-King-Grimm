using Assets.Scripts;
using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.DataTypes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class FirebatFirePillar : MonoBehaviour, IPoolableObject
{
	static ObjectPool<FirebatFirePillar> Pool;
	public float time = 0f;

	class Hook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			Pool = ObjectPool<FirebatFirePillar>.CreatePool(grimm.Prefabs.FirebatPillarPrefab, ObjectPoolStorageType.ActiveSceneOnly, 5);
		}
	}

	public static FirebatFirePillar Spawn(InfernoKingGrimm grimm, bool autoPlay = true)
	{
		//var fireBatPillar = GameObject.Instantiate(grimm.Prefabs.FirebatPillarPrefab,grimm.transform,false).GetComponent<FirebatFirePillar>();
		var fireBatPillar = Pool.RetrieveFromPool();
		fireBatPillar.transform.parent = grimm.transform;
		fireBatPillar.transform.localPosition = grimm.Prefabs.FirebatPillarPrefab.transform.position;

		fireBatPillar.gameObject.SetActive(false);

		if (grimm.FaceDirection == GrimmDirection.Left)
		{
			fireBatPillar.transform.localPosition = new Vector3(-fireBatPillar.transform.localPosition.x, fireBatPillar.transform.localPosition.y, fireBatPillar.transform.localPosition.z);
		}

		if (autoPlay)
		{
			fireBatPillar.Play();
		}
		return fireBatPillar;
	}

	void Update()
	{
		time += Time.deltaTime;
	}

	public void Play()
	{
		gameObject.SetActive(true);
		foreach (var particles in GetComponentsInChildren<ParticleSystem>())
		{
			particles.Play();
		}
		//Destroy(gameObject, 10.0f);
		Pool.ReturnToPool(this, 0.71f);
	}

	public void Stop()
	{
		gameObject.SetActive(false);
	}

	void IPoolableObject.OnPool()
	{
		
	}
}
