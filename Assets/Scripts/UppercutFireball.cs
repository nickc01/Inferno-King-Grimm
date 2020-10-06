using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WeaverCore;
using WeaverCore.DataTypes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class UppercutFireball : MonoBehaviour, IPoolableObject
{
	static ObjectPool<UppercutFireball> Pool;

	[SerializeField]
	float lifeTime = 1.5f;

	float counter = 0f;

	ColliderDisabler disabler;
	CircleCollider2D circleCollider;
	Rigidbody2D _rigidBody;
	public Rigidbody2D RigidBody
	{
		get
		{
			if (_rigidBody == null)
			{
				_rigidBody = GetComponent<Rigidbody2D>();
			}
			return _rigidBody;
		}
	}

	void Awake()
	{
		if (circleCollider == null)
		{
			circleCollider = GetComponent<CircleCollider2D>();
		}
		//WeaverLog.Log("Enabling Collider = " + circleCollider.enabled);
		circleCollider.enabled = true;
	}

	void IPoolableObject.OnPool()
	{
		RigidBody.velocity = Vector2.zero;
		if (disabler == null)
		{
			disabler = GetComponent<ColliderDisabler>();
		}
		disabler.clock = 0f;
	}

	void Update()
	{
		counter += Time.deltaTime;
		if (counter >= lifeTime)
		{
			Pool.ReturnToPool(this);
		}
	}

	public static UppercutFireball Create(Vector3 position, Quaternion rotation = default(Quaternion))
	{
		if (rotation == default(Quaternion))
		{
			rotation = Quaternion.identity;
		}
		return Pool.RetrieveFromPool(position, rotation);
	}

	class UppercutBallHook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			Pool = ObjectPool<UppercutFireball>.CreatePool(grimm.Prefabs.UppercutFireball, ObjectPoolStorageType.ActiveSceneOnly, 8);
		}
	}
}
