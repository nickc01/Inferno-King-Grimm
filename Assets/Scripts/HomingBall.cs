using Assets.Scripts;
using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class HomingBall : MonoBehaviour, IPoolableObject
{
	new Rigidbody2D rigidbody;
	new Collider2D collider;
	ParticleSystem Smoke;
	ParticleSystem Particles;

	Coroutine MainCoroutine;
	Coroutine ShrinkCoroutine;
	[Header("Phase 1")]
	public bool EnablePhase1 = true;
	[FormerlySerializedAs("SpawnTime")]
	public float Phase1Time = 1f;
	[FormerlySerializedAs("SpawnVelocity")]
	public float Phase1Velocity = 20f;
	public Vector2 Phase1TravelDirection = default(Vector2);

	[Header("Phase 2")]
	public bool EnablePhase2 = true;
	[FormerlySerializedAs("RotationSpeed")]
	public float Phase2RotationSpeed = 25f;
	[FormerlySerializedAs("Velocity")]
	public float Phase2Velocity = 5f;
	[FormerlySerializedAs("TargetOffset")]
	public Vector2 Phase2TargetOffset = default(Vector2);
	public Vector2 Phase2TravelDirection = default(Vector2);

	[Header("Death")]
	[FormerlySerializedAs("shrinkTime")]
	[Tooltip("How fast the homing ball shrinks when it hits an obstacle")]
	public float ShrinkTime = 0.7f;

	public static Vector2 GlowScale = Vector2.one;
	public static Vector2 FirePillarOffset = Vector2.zero;
	static ObjectPool<HomingBall> HomingBallPool;

	public static HashSet<HomingBall> ActiveHomingBalls = new HashSet<HomingBall>();

	Growth growthComponent;

	//TODO : TODO : TODO : TODO : TODO : TODO : TODO : TODO :TODO : TODO : TODO : TODO :TODO : TODO : TODO : TODO :
	//public Vector2 travelDirection = Vector2.up;

	float _lifeTime = 0f;

	[SerializeField]
	float LifeTime = 5f;

	class HomingBallHook : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			HomingBallPool = ObjectPool<HomingBall>.CreatePool(grimm.Prefabs.HomingBallPrefab, ObjectPoolStorageType.ActiveSceneOnly, 20);
		}
	}

	void Start()
	{
		//Debug.Log("Start Called");
		if (rigidbody == null)
		{
			rigidbody = GetComponent<Rigidbody2D>();
			collider = GetComponent<Collider2D>();
			Smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
			Particles = transform.Find("Particles").GetComponent<ParticleSystem>();
			growthComponent = GetComponent<Growth>();
		}
		Smoke.Play();
		Particles.Play();
		//Debug.Log("Starting Main Coroutine");
		MainCoroutine = StartCoroutine(MainAction());
	}

	void Update()
	{
		_lifeTime += Time.deltaTime;
	}

	public void ShrinkAndStop()
	{
		if (MainCoroutine != null)
		{
			StopCoroutine(MainCoroutine);
			MainCoroutine = null;
		}
		if (ShrinkCoroutine == null && gameObject.activeSelf)
		{
			ShrinkCoroutine = StartCoroutine(ShrinkRoutine());
		}
	}

	IEnumerator MainAction()
	{
		if (EnablePhase1)
		{
			float phase1Timer = 0f;

			while (phase1Timer < Phase1Time && _lifeTime < LifeTime)
			{
				transform.position += (Vector3)Vector2.Lerp(Phase1TravelDirection * Phase1Velocity, Vector2.zero, phase1Timer / Phase1Time) * Time.deltaTime;

				yield return null;

				phase1Timer += Time.deltaTime;
			}

			Phase1TravelDirection = Vector3.zero;
		}
		if (EnablePhase2)
		{
			while (_lifeTime < LifeTime)
			{
				transform.position += (Vector3)Phase2TravelDirection * Time.deltaTime;
				Phase2TravelDirection = Vector3.RotateTowards(Phase2TravelDirection, ((Player.Player1.transform.position + (Vector3)Phase2TargetOffset) - transform.position) * 100f, Phase2RotationSpeed * Mathf.Deg2Rad * Time.deltaTime, Phase2Velocity * Time.deltaTime);
				yield return null;
			}
		}

		ShrinkAndStop();
	}

	Vector2 Difference(Vector2 a, Vector2 b)
	{
		return new Vector2(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
	}

	IEnumerator ShrinkRoutine()
	{
		Smoke.Stop();
		Particles.Stop();
		collider.enabled = false;
		Vector3 oldScale = transform.localScale;

		float clock = 0f;
		float end = 0.5f;
		do
		{
			yield return null;
			clock += Time.deltaTime;
			transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, clock / ShrinkTime);
			rigidbody.velocity *= 0.85f;
		} while (clock < end);

		//Destroy(gameObject);
		HomingBallPool.ReturnToPool(this);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (_lifeTime >= 0.3f && collision.gameObject.layer == LayerMask.NameToLayer("Terrain") && ShrinkCoroutine == null)
		{
			ShrinkAndStop();
		}
	}

	public static HomingBall Fire(InfernoKingGrimm grimm, Vector3 Position, float spawnAngle, float spawnVelocity, float rotationSpeed, bool playEffects = true, float audioPitch = 1f)
	{
		return Fire(grimm, Position, VectorUtilities.DegreesToVector(spawnAngle, spawnVelocity), rotationSpeed, playEffects, audioPitch);
	}

	public static HomingBall Fire(InfernoKingGrimm grimm, Vector3 Position, Vector2 spawnVector, float rotationSpeed, bool playEffects = true, float audioPitch = 1f)
	{
		//var newBall = Instantiate(MainPrefabs.Instance.HomingBallPrefab,Position,Quaternion.identity);
		var newBall = HomingBallPool.RetrieveFromPool(Position, Quaternion.identity);
		//Debug.Log("Starting Lifetime = " + newBall._lifeTime);

		if (grimm.FaceDirection == GrimmDirection.Left)
		{
			spawnVector = spawnVector.With(-spawnVector.x);
		}
		newBall.Phase1TravelDirection = spawnVector.normalized;
		newBall.Phase1Velocity = spawnVector.magnitude;
		newBall.Phase2RotationSpeed = rotationSpeed;

		if (playEffects)
		{
			var pillar = FirebatFirePillar.Spawn(grimm);
			if (FirePillarOffset != Vector2.zero)
			{
				pillar.transform.position = Position + (Vector3)FirePillarOffset;
			}

			var fireAudio = WeaverAudio.Play(grimm.Sounds.GrimmBatFire, grimm.transform.position, 1.0f, AudioChannel.Sound);
			fireAudio.AudioSource.pitch = audioPitch;

			//var glow = GameObject.Instantiate(MainPrefabs.Instance.GlowPrefab, Position + new Vector3(0f, 0f, -0.1f), Quaternion.identity);
			var glow = GrimmGlow.Create(Position + new Vector3(0f, 0f, -0.1f));
			glow.transform.localScale = GlowScale;
		}

		ActiveHomingBalls.Add(newBall);
		return newBall;
	}

	void IPoolableObject.OnPool()
	{
		ActiveHomingBalls.Remove(this);
		transform.localScale = HomingBallPool.ObjectToPool.transform.localScale;
		//_lifeTime = 0f;
		/*if (MainCoroutine != null)
		{
			StopCoroutine(MainCoroutine);
			MainCoroutine = null;
		}
		if (ShrinkCoroutine != null)
		{
			StopCoroutine(ShrinkCoroutine);
			ShrinkCoroutine = null;
		}*/
		MainCoroutine = null;
		ShrinkCoroutine = null;
		collider.enabled = true;
		//EnablePhase1 = true;
		//Phase1Time = 1f;
		//Phase1Velocity = 20f;
		//Phase1TravelDirection = default(Vector2);
		//EnablePhase2 = true;
		//Phase2RotationSpeed = 25f;
		//Phase2Velocity = 5f;
		//Phase2TargetOffset = default(Vector2);
		//Phase2TravelDirection = default(Vector2);
		growthComponent.enabled = false;
		growthComponent.lifeTime = 0f;
		//ShrinkTime = 0.7f;
	}

	void OnDestroy()
	{
		ActiveHomingBalls.Remove(this);
	}
}
