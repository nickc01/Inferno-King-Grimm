using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.DataTypes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class GrimmGlow : MonoBehaviour, IPoolableObject 
{
	static ObjectPool<GrimmGlow> Pool;

	[SerializeField]
	string startingStateName;
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
		animator.Play(startingStateName);
	}

	class Hooks : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			Pool = ObjectPool<GrimmGlow>.CreatePool(grimm.Prefabs.GlowPrefab, ObjectPoolStorageType.ActiveSceneOnly, 3, true);
		}
	}

	void IPoolableObject.OnPool()
	{
		
	}

	public void Destroy()
	{
		Pool.ReturnToPool(this);
	}

	public static GrimmGlow Create(Vector3 position)
	{
		return Pool.RetrieveFromPool(position, Quaternion.identity);
	}
}
