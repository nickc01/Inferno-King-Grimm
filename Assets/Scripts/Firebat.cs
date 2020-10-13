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

public class Firebat : MonoBehaviour, IOnPool 
{
	static ObjectPool FirebatPool;

	/*class Hook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			FirebatPool = new Pool(grimm.Prefabs.FirebatPillarPrefab, PoolType.Local);
			FirebatPool.FillPoolAsync(6);
		}
	}*/

	[OnIKGAwake]
	static void OnGrimmStart()
	{
		FirebatPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.FirebatPrefab, PoolLoadType.Local);
		FirebatPool.FillPoolAsync(6);
	}

	public float Angle { get; private set; }
	public GrimmDirection GeneralDirection { get; private set; }
	public float DirectionAngle { get; private set; }

	float XVelocity = 0.0f;

	new Rigidbody2D rigidbody;
	new SpriteRenderer renderer;
	static Transform _fbSpawnPoint;
	static Transform FirebatSpawnPoint
	{
		get
		{
			if (_fbSpawnPoint == null)
			{
				_fbSpawnPoint = InfernoKingGrimm.Instance.transform.Find("Firebat SpawnPoint");
			}
			return _fbSpawnPoint;
		}
	}

	// Use this for initialization
	void Start () 
	{
		//Destroy(gameObject, 3);
		StartCoroutine(Waiter(3));
	}

	IEnumerator Waiter(float time)
	{
		yield return new WaitForSeconds(time);
		FirebatPool.ReturnToPool(this);
	}
	
	// Update is called once per frame
	void Update () 
	{
		rigidbody.velocity = Vector3.RotateTowards(rigidbody.velocity, Player.NearestPlayer(this).transform.position - transform.position, 30.0f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);
		//rigidbody.velocity.
	}

	public static Firebat Spawn(float angle, float velocity, GrimmDirection direction, Vector3 position)
	{
		//Debugger.Log("Fire bat F");
		//var fireBat = FirebatPool.RetrieveFromPool(position, Quaternion.identity);
		var fireBat = FirebatPool.Instantiate<Firebat>(position, Quaternion.identity);

		if (fireBat.rigidbody == null)
		{
			fireBat.rigidbody = fireBat.GetComponent<Rigidbody2D>();
			fireBat.renderer = fireBat.GetComponent<SpriteRenderer>();
		}

		//fireBat.Altitude = altitude;
		fireBat.Angle = angle;
		fireBat.XVelocity = velocity;
		fireBat.GeneralDirection = direction;

		if (direction == GrimmDirection.Right)
		{
			fireBat.SetVelocity(new Vector2(velocity, 0));
			fireBat.renderer.flipX = true;
		}
		else
		{
			fireBat.SetVelocity(new Vector2(-velocity, 0));
		}

		if (direction == GrimmDirection.Right)
		{
			fireBat.DirectionAngle = angle;
		}
		else
		{
			fireBat.DirectionAngle = 180 - angle;
		}
		fireBat.SetVelocity(fireBat.DirectionAngle.DegreesToVector() * velocity);
		//Debugger.Log("Fire bat G");
		return fireBat;
	}

	public static Firebat Spawn(float angle, float velocity, InfernoKingGrimm grimm, Vector3? position = null)
	{
		//Debugger.Log("Fire bat D");
		if (position == null)
		{
			position = FirebatSpawnPoint.position;
		}
		//Debugger.Log("Fire bat E");
		return Spawn(angle, velocity, grimm.FaceDirection,position.Value);
	}

	public static IEnumerator SendFirebatAsync(InfernoKingGrimm grimm, float angle, float audioPitch = 1.0f, float speedMultiplier = 1f, bool playSound = true)
	{
		//Debugger.Log("FIre bat C");
		var fireBatVelocity = grimm.transform.localScale.x * 20f * speedMultiplier;

		Spawn(angle, fireBatVelocity, grimm);
		FirebatFirePillar.Spawn(grimm);
		if (playSound)
		{
			var fireAudio = WeaverAudio.PlayAtPoint(grimm.Sounds.GrimmBatFire, grimm.transform.position, 1.0f, AudioChannel.Sound);
			fireAudio.AudioSource.pitch = audioPitch;
		}

		GrimmGlow.Create(FirebatSpawnPoint.position + new Vector3(0f, 0f, -0.1f));

		yield return new WaitForSeconds(0.3f);

		yield break;
	}

	public static void SendFirebat(InfernoKingGrimm grimm, float angle, float pitch = 1.0f, float speedMultiplier = 1f, bool playSound = true)
	{
		UnboundCoroutine.Start(SendFirebatAsync(grimm, angle, pitch, speedMultiplier,playSound));
	}


	void SetVelocity(Vector2 velocity)
	{
		rigidbody.velocity = velocity;
	}

	void IOnPool.OnPool()
	{
		renderer.flipX = false;
		renderer.flipY = false;
		rigidbody.velocity = Vector2.zero;
	}
}
