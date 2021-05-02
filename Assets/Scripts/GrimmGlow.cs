using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class GrimmGlow : MonoBehaviour
{
	//static ObjectPool GlowPool;

	//[SerializeField]
	//string startingStateName;
	Animator animator;

	void Awake()
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
	}

	void Start()
	{
		animator.Play("Default");
	}

	/*class Hooks : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			GlowPool = new Pool(grimm.Prefabs.GlowPrefab, PoolType.Local);
			GlowPool.FillPoolAsync(1);
			//Pool = Pool.CreatePool(grimm.Prefabs.GlowPrefab, ObjectPoolStorageType.ActiveSceneOnly, 3, true);
		}
	}*/

	/*[OnIKGAwake]
	static void OnGrimmAwake()
	{
		GlowPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.GlowPrefab, PoolLoadType.Local);
		GlowPool.FillPoolAsync(1);
	}*/

	public void Destroy()
	{
		//Pool.ReturnToPool(this);
		//GlowPool.ReturnToPool(this);
		Pooling.Destroy(this);
	}

	public static GrimmGlow Create(Vector3 position)
	{
		//return Pool.RetrieveFromPool(position, Quaternion.identity);
		return Pooling.Instantiate<GrimmGlow>(InfernoKingGrimm.MainGrimm.Prefabs.GlowPrefab, position, Quaternion.identity);
	}
}
