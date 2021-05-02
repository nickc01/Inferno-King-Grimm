using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class Pillar : MonoBehaviour, IOnPool
{
	//static ObjectPool PillarPool;
	/*class Hook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			//Pool = ObjectPool<Pillar>.CreatePool(grimm.Prefabs.FlamePillarPrefab, ObjectPoolStorageType.ActiveSceneOnly, 4);
			PillarPool = new Pool(grimm.Prefabs.FlamePillarPrefab,PoolType.Local);
			PillarPool.FillPoolAsync(4);
		}
	}*/

	/*[OnIKGAwake]
	static void OnGrimmAwake()
	{
		PillarPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.FlamePillarPrefab, PoolLoadType.Local);
		PillarPool.FillPoolAsync(4);
	}*/

	[SerializeField]
	float PillarSpawnY = 5.1f;



	[SerializeField]
	AudioClip FlameExplode;

	DamageHero damager;
	ParticleSystem AfterBurn;
	CircleCollider2D AfterBurnCollider;

	public bool PlaySound = true;
	public float Volume = 1f;
	public float FadeOutTime = 1f;

	public bool DamagePlayer
	{
		get
		{
			return damager.enabled;
		}
		set
		{
			damager.enabled = value;
		}
	}

	void Awake()
	{
		if (damager == null)
		{
			damager = GetComponentInChildren<DamageHero>(true);
			AfterBurn = transform.Find("Pt Afterburn").GetComponent<ParticleSystem>();
			AfterBurnCollider = AfterBurn.GetComponent<CircleCollider2D>();
		}
	}

	void Start() 
	{
		StartCoroutine(PillarRoutine());
	}

	IEnumerator PillarRoutine()
	{
		transform.position = new Vector3(transform.position.x ,PillarSpawnY,transform.position.z);
		//WeaverAudio.Play(FlameChargeSound, transform.position);

		yield return new WaitForSeconds(0.5f);

		CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

		if (PlaySound)
		{
			var explosion = WeaverAudio.PlayAtPoint(FlameExplode, transform.position);
			explosion.Volume = Volume;
		}

		yield return new WaitForSeconds(0.3f);

		//var afterBurnObject = transform.Find("Pt Afterburn").gameObject;

		//var afterBurn = afterBurnObject.GetComponent<ParticleSystem>();
		//afterBurn.Play();
		AfterBurn.Play();

		//var collider = afterBurnObject.GetComponent<CircleCollider2D>();

		//collider.enabled = true;
		AfterBurnCollider.enabled = true;

		yield return new WaitForSeconds(FadeOutTime);

		//afterBurn.Stop();
		AfterBurn.Stop();

		//collider.enabled = false;
		AfterBurnCollider.enabled = false;

		yield return new WaitForSeconds(0.22f);

		//Destroy(gameObject);
		Pooling.Destroy(this);
		//PillarPool.ReturnToPool(this);

		//yield break;
	}

	void IOnPool.OnPool()
	{
		AfterBurnCollider.enabled = false;
		AfterBurn.Stop();
	}

	public static Pillar Create(Vector3 position)
	{
		return Pooling.Instantiate<Pillar>(InfernoKingGrimm.MainGrimm.Prefabs.FlamePillarPrefab, position, Quaternion.identity);
	}
}
