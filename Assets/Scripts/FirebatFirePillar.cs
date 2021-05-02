using Assets.Scripts;
using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class FirebatFirePillar : MonoBehaviour
{
	//static ObjectPool PillarPool;

	ParticleSystem[] particleSystems;
	float time = 0f;

	/*class Hook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			//Pool = ObjectPool<FirebatFirePillar>.CreatePool(grimm.Prefabs.FirebatPillarPrefab, ObjectPoolStorageType.ActiveSceneOnly, 5);
			PillarPool = new Pool(grimm.Prefabs.FirebatPillarPrefab, PoolType.Local);
			PillarPool.FillPoolAsync(3);
		}
	}*/

	/*[OnIKGAwake]
	static void OnGrimmAwake()
	{
		PillarPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.FirebatPillarPrefab, PoolLoadType.Local);
		PillarPool.FillPoolAsync(3);
	}*/

	public static FirebatFirePillar Spawn(InfernoKingGrimm grimm, bool autoPlay = true)
	{
		//var fireBatPillar = GameObject.Instantiate(grimm.Prefabs.FirebatPillarPrefab,grimm.transform,false).GetComponent<FirebatFirePillar>();
		//var fireBatPillar = Pool.RetrieveFromPool();
		var fireBatPillar = Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.FirebatPillarPrefab);
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

	void Awake()
	{
		if (particleSystems == null)
		{
			particleSystems = GetComponentsInChildren<ParticleSystem>();
		}
	}

	void Update()
	{
		time += Time.deltaTime;
	}

	public void Play()
	{
		gameObject.SetActive(true);
		foreach (var particles in particleSystems)
		{
			particles.Play();
		}
		Pooling.Destroy(this);
		//Destroy(gameObject, 10.0f);
		//PillarPool.ReturnToPool(this, 0.71f);
	}

	public void Stop()
	{
		gameObject.SetActive(false);
	}
}
