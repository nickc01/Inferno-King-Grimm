using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class HomingBall : MonoBehaviour 
{
	new Rigidbody2D rigidbody;
	new Collider2D collider;
	ParticleSystem Smoke;
	ParticleSystem Particles;

	Coroutine MainCoroutine;
	Coroutine ShrinkCoroutine;

	[SerializeField]
	float shrinkTime = 0.7f;

	public float Velocity = 5f;

	[SerializeField]
	public float RotationSpeed = 25f;

	public float SpawnTime = 1f;
	public float SpawnVelocity = 20f;

	public Vector2 TargetOffset = default(Vector2);

	Vector2 travelDirection = Vector2.up;

	void Start () 
	{
		rigidbody = GetComponent<Rigidbody2D>();
		collider = GetComponent<Collider2D>();
		Smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
		Particles = transform.Find("Particles").GetComponent<ParticleSystem>();
		MainCoroutine = StartCoroutine(MainAction());
	}

	void ShrinkAndStop()
	{
		if (MainCoroutine != null)
		{
			StopCoroutine(MainCoroutine);
			MainCoroutine = null;
		}
		if (ShrinkCoroutine == null)
		{
			ShrinkCoroutine = StartCoroutine(ShrinkRoutine());
		}
	}

	IEnumerator MainAction()
	{
		float time = 0f;

		Vector2 spawnVelocityVector = travelDirection * SpawnVelocity;
		//Vector2 currentVelocityVector = spawnVelocityVector;

		while (time < SpawnTime)
		{
			transform.position += (Vector3)Vector2.Lerp(spawnVelocityVector, Vector2.zero, time / SpawnTime) * Time.deltaTime;

			//transform.position += (Vector3)currentVelocityVector * Time.deltaTime;

			//currentVelocityVector = Vector2.Lerp(spawnVelocityVector, Vector2.zero,time / SpawnTime);

			yield return null;

			time += Time.deltaTime;
		}

		travelDirection = Vector3.zero;

		while (true)
		{
			//rigidbody.velocity += Difference(transform.position, Player.Player1.transform.position).normalized * -(acceleration * Time.deltaTime);
			transform.position += (Vector3)travelDirection * Time.deltaTime;
			travelDirection = Vector3.RotateTowards(travelDirection, ((Player.Player1.transform.position + (Vector3)TargetOffset) - transform.position) * 100f, RotationSpeed * Mathf.Deg2Rad * Time.deltaTime, Velocity * Time.deltaTime);
			yield return null;
		}
	}

	Vector2 Difference(Vector2 a, Vector2 b)
	{
		return new Vector2(Mathf.Abs(a.x - b.x),Mathf.Abs(a.y - b.y));
	}

	IEnumerator ShrinkRoutine()
	{
		//Debugger.Log("Stopping");
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
			transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, clock / shrinkTime);
			rigidbody.velocity *= 0.85f;
		} while (clock < end);

		Destroy(gameObject);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") && ShrinkCoroutine == null)
		{
			ShrinkAndStop();
		}
	}

	public static HomingBall Fire(InfernoKingGrimm grimm, Vector3 Position, float spawnAngle, float spawnVelocity, float rotationSpeed, bool spawnFlamePillar = true)
	{
		return Fire(grimm, Position, VectorUtilities.AngleToVector(spawnAngle * Mathf.Deg2Rad,spawnVelocity), rotationSpeed, spawnFlamePillar);
	}

	public static HomingBall Fire(InfernoKingGrimm grimm, Vector3 Position, Vector2 spawnVector, float rotationSpeed, bool spawnFlamePillar = true)
	{
		var newBall = Instantiate(MainPrefabs.Instance.HomingBallPrefab,Position,Quaternion.identity);

		if (grimm.FaceDirection == GrimmDirection.Left)
		{
			spawnVector = spawnVector.With(-spawnVector.x);
		}
		newBall.travelDirection = spawnVector.normalized;
		newBall.SpawnVelocity = spawnVector.magnitude;
		newBall.RotationSpeed = rotationSpeed;

		if (spawnFlamePillar)
		{
			FirebatFirePillar.Spawn(grimm);

			var fireAudio = WeaverAudio.Play(grimm.Sounds.GrimmBatFire, grimm.transform.position, 1.0f, AudioChannel.Sound);
			fireAudio.AudioSource.pitch = 1.0f;
		}

		return newBall;
	}

	/*void OnTriggerEnter2D(Collider2D collider)
	{
		Debugger.Log("Trigger = " + collider);
	}*/
}
