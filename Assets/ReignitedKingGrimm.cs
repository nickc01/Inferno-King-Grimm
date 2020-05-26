using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.GameStatus;
using WeaverCore.Helpers;

using Random = UnityEngine.Random;


public enum Altitude
{
	High,
	Low
}

public enum Direction
{
	Right,
	Left
}



public enum GrimmAttackMode
{
	None,
	Firebats, //A move where grimm teleports to an edge of the map and shoots firebats
	GroundSlash, //A move where grimm teleports to an edge of the map, does a slash, and them shoots upwards
	AirDash, //A move where grimm teleports in the air, dashes to the player, and then dashes again on the ground
	Spikes,
	FirePillars,
	Balloon
}


[RequireComponent(typeof(DamageHero))]
public class ReignitedKingGrimm : EnemyReplacement
{
	public static readonly Vector3 CentralPosition = new Vector3(85.77f,13.5f,0.0f);
	//public static float GroundY = 8.3f;

	public GrimmAttackMode CurrentAttackMode { get; private set; }


	PolygonCollider2D DashSpike;
	new Rigidbody2D rigidbody;
	WeaverAudioPlayer VoicePlayer;
	EventReceiver receiver;
	new SpriteRenderer renderer;
	Animator animator;
	new BoxCollider2D collider;
	GrimmSounds Sounds;
	FirebatFirePillar firebatPillar;

	ParticleSystem teleSmokeBack;
	ParticleSystem teleSmokeFront;

	GameObject FirebatSpawnpoint;
	float fireBatSpawnpointX;

	bool invisible = true;

	[Space]
	[Space]
	[Header("Prefabs")]
	[SerializeField]
	MainPrefabs prefabs;


	[Header("Arena Settings")]
	[SerializeField]
	float groundY = 8.3f;
	[SerializeField]
	float leftEdge = 69.7f;
	[SerializeField]
	float rightEdge = 102.23f;

	[Space]
	[Space]
	[Header("Firebat Settings")]
	[SerializeField]
	float highAngle = 25f;
	[SerializeField]
	float lowAngle = -15f;

	[Header("Air Dash Settings")]
	[SerializeField]
	float airDashSpeed = 55f;
	[SerializeField]
	float flameSpawnRate = 0.025f;
	[SerializeField]
	Vector3 flameSpawnOffset = new Vector3(0f,-1f,0.001f);
	[SerializeField]
	float groundDashSpeed = 58f;
	[SerializeField]
	float groundDashTime = 0.25f;

	public static MainPrefabs Prefabs { get; private set; }

	public Direction FaceDirection { get; private set; }


	void Awake()
	{
		Prefabs = prefabs;
	}

	// Use this for initialization
	protected override void Start () 
	{
		base.Start();

		Debugger.Log("ENEMY HAS STARTED");

		if ((receiver = GetComponent<EventReceiver>()) == null)
		{
			receiver = gameObject.AddComponent<EventReceiver>();
		}

		DashSpike = GetComponentInChildren<PolygonCollider2D>();
		rigidbody = GetComponent<Rigidbody2D>();
		VoicePlayer = GetComponent<WeaverAudioPlayer>();
		Sounds = GetComponentInChildren<GrimmSounds>();
		renderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		collider = GetComponent<BoxCollider2D>();

		FirebatSpawnpoint = transform.Find("Firebat SpawnPoint").gameObject;

		fireBatSpawnpointX = FirebatSpawnpoint.transform.localPosition.x;

		teleSmokeBack = transform.Find("tele_smoke_back").GetComponent<ParticleSystem>();
		teleSmokeFront = transform.Find("tele_smoke_front").GetComponent<ParticleSystem>();

		if (GameStatus.GameState == RunningState.Game)
		{
			receiver.ReceiveAllEventsFromName("WAKE");
			receiver.OnReceiveEvent += Wake;
		}
		else
		{
			StartCoroutine(Waiter(1,() => Wake("WAKE")));
		}

		Debugger.Log("Random Attack Mode = " + GetRandomAttackMode());

		animator.enabled = false;
		renderer.enabled = false;
		collider.enabled = false;
	}

	IEnumerator PlayAnimationTillDone(string animationStateName)
	{
		animator.Play(animationStateName);

		yield return null;

		var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		Debugger.Log("Duration = " + stateInfo.length);
		Debugger.Log("Speed = " + stateInfo.speed);

		yield return new WaitForSeconds(stateInfo.length / stateInfo.speed);
	}

	void PlayAnimation(string animationStateName)
	{
		animator.Play(animationStateName);
	}

	IEnumerator Waiter(float waitTime, Action OnDone)
	{
		yield return new WaitForSeconds(waitTime);
		OnDone();
	}

	//Called when the boss starts
	void Wake(string eventName)
	{
		Debugger.Log("THE ENEMY HAS AWOKEN!!!");
		transform.position = CentralPosition;

		StartCoroutine(MainBossControl());
	}



	IEnumerator MainBossControl()
	{
		yield return new WaitForSeconds(0.6f);
		while (true)
		{
			yield return AirDashMove();
			//yield return FirebatsMove();
			/*switch (GetRandomAttackMode())
			{

				case GrimmAttackMode.Firebats:
					yield return FirebatsMove();
					break;
				case GrimmAttackMode.GroundSlash:
					yield return GroundSlashMove();
					break;
				case GrimmAttackMode.AirDash:
					yield return AirDashMove();
					break;
				case GrimmAttackMode.Spikes:
					yield return SpikesMove();
					break;
				case GrimmAttackMode.FirePillars:
					yield return PillarsMove();
					break;
			}*/

			//yield return TeleportIn();

			//yield return new WaitForSeconds(1.0f);

			//yield return TeleportOut();

			//yield return new WaitForSeconds(1.0f);
		}
	}

	IEnumerator FirebatsMove()
	{
		var playerPos = Player.Player1.transform.position;
		if (playerPos.x <= 88)
		{
			transform.position = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), groundY);
		}
		else
		{
			transform.position = new Vector3(UnityEngine.Random.Range(74.0f,77.0f),groundY);
		}

		FacePlayer(true);

		yield return TeleportIn();

		PlayAnimation("Bat Cast Charge");


		//WeaverAudio.Play(Sounds.GrimmBeginBatCastVoice, transform.position);
		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);

		yield return new WaitForSeconds(0.2f);

		PlayAnimation("Cast");

		yield return new WaitForSeconds(0.3f);
		yield return SendFirebat(Altitude.High, 1.0f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebat(Altitude.Low, 1.1f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebat(Altitude.High, 1.2f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebat(Altitude.Low, 1.3f, 0.75f);

		yield return new WaitForSeconds(0.3f);

		PlayAnimation("Cast Return");

		if (UnityEngine.Random.Range(0,1) == 0)
		{
			yield return new WaitForSeconds(0.5f);
		}

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator BackupFirebatMove()
	{
		yield return TeleportOut();

		var playerPos = Player.Player1.transform.position;
		if (playerPos.x <= 88)
		{
			transform.position = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), groundY);
		}
		else
		{
			transform.position = new Vector3(UnityEngine.Random.Range(74.0f, 77.0f), groundY);
		}

		FacePlayer(true);

		yield return TeleportIn();

		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);

		PlayAnimation("Bat Cast Charge");

		yield return new WaitForSeconds(0.2f);

		PlayAnimation("Cast");

		yield return new WaitForSeconds(0.3f);
		StartCoroutine(SendFirebat(Altitude.High, 1.0f));
		yield return SendFirebat(Altitude.Low, 1.0f);

		PlayAnimation("Cast Return");

		yield return new WaitForSeconds(0.5f);

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator GroundSlashMove()
	{
		yield break;
	}

	IEnumerator AirDashMove()
	{
		Debugger.Log("A");

		float teleX = float.PositiveInfinity;
		do
		{
			var playerPos = Player.Player1.transform.position;

			var teleAdder = Random.Range(7.5f, 9f);

			if (Random.value > 0.5f)
			{
				teleAdder = -teleAdder;
			}

			teleX = teleAdder + playerPos.x;

			Debugger.Log("Tele X = " + teleX);

			if (teleX < 75f || teleX > 99f)
			{
				teleX = float.PositiveInfinity;
				continue;
			}

			yield return null;
		}
		while (teleX == float.PositiveInfinity);

		FacePlayer();

		Debugger.Log("B");

		transform.position = new Vector3(teleX, 15f, 0f);

		yield return TeleportIn();

		Debugger.Log("Angle To Player = " + GetAngleTo(transform.position, Player.Player1.transform.position));

		var velocityAngle = Mathf.Clamp(GetAngleTo(transform.position, Player.Player1.transform.position),-135f,-45f);
		var spriteAngle = velocityAngle + 90f;

		Debugger.Log("Angle = " + velocityAngle);
		//WeaverAudio.Play(Sounds.AirDashAntic, transform.position, 1, AudioChannel.Sound);

		//VoicePlayer.Play();

		yield return PlayAnimationTillDone("Air Dash Antic");

		PlayAnimation("Air Dash");


		rigidbody.velocity = VectorTools.PolarToCartesian(velocityAngle * Mathf.Deg2Rad, airDashSpeed);
		transform.eulerAngles = new Vector3(0f,0f,spriteAngle);



		WeaverAudio.Play(Sounds.AirDash, transform.position);



		var effect = Instantiate(Prefabs.AirDashEffect, transform, false);
		effect.transform.parent = null;


		float fireTimer = 0f;

		bool vertical = false;

		do
		{
			yield return null;
			fireTimer += Time.deltaTime;
			if (fireTimer >= flameSpawnRate)
			{
				fireTimer -= flameSpawnRate;
				Instantiate(Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset,Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (!vertical && (transform.position.x <= leftEdge || transform.position.x >= rightEdge))
			{
				rigidbody.velocity = VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				transform.eulerAngles = Vector3.zero;
				vertical = true;
			}
		} while (transform.position.y > groundY);


		rigidbody.velocity = Vector2.zero;

		transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

		PlayAnimation("Ground Dash Antic");

		var landPlayer = WeaverAudio.Play(Sounds.LandSound, transform.position);
		landPlayer.AudioSource.pitch = 0.9f;

		//TODO : Make Camera Shake

		FacePlayer();
		transform.rotation = Quaternion.identity;

		Instantiate(Prefabs.SlamEffect, transform.position + Prefabs.SlamEffect.transform.position, Quaternion.identity);

		yield return new WaitForSeconds(0.5f);

		WeaverAudio.Play(Sounds.GroundDash, transform.position);

		var groundEffect = Instantiate(Prefabs.GroundDashEffect, transform.position + Prefabs.GroundDashEffect.transform.position, Quaternion.identity);
		var dashSpeed = transform.localScale.x * groundDashSpeed;

		if (FaceDirection == Direction.Left)
		{
			groundEffect.GetComponent<SpriteRenderer>().flipX = true;
			groundEffect.transform.position -= Prefabs.GroundDashEffect.transform.position * 2f;
			dashSpeed = -dashSpeed;
		}

		rigidbody.velocity = new Vector2(dashSpeed,0f);

		PlayAnimation("G Dash");

		DashSpike.enabled = true;

		var dustEffect = Instantiate(Prefabs.DustGroundEffect, transform.position + Prefabs.DustGroundEffect.transform.position, Prefabs.DustGroundEffect.transform.rotation).GetComponent<ParticleSystem>();

		fireTimer = 0f;
		float waitTimer = 0f;

		bool horizontal = true;

		do
		{
			yield return null;
			fireTimer += Time.deltaTime;
			waitTimer += Time.deltaTime;
			if (fireTimer >= flameSpawnRate)
			{
				fireTimer -= flameSpawnRate;
				Instantiate(Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (!horizontal && (transform.position.x <= leftEdge || transform.position.x >= rightEdge))
			{
				rigidbody.velocity = Vector2.zero;//VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				horizontal = false;
			}
		} while (waitTimer < groundDashTime);

		PlayAnimation("Ground Dash Antic");

		waitTimer = 0f;

		DashSpike.enabled = false;

		dustEffect.Stop();

		do
		{
			yield return null;
			rigidbody.velocity = Decelerate(rigidbody.velocity, new Vector2(0.75f,0f));
			waitTimer += Time.deltaTime;
		} while (waitTimer < 0.33f);

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator SpikesMove()
	{
		yield break;
	}

	IEnumerator PillarsMove()
	{
		yield break;
	}

	IEnumerator BalloonMove()
	{
		yield break;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	IEnumerator SendFirebat(Altitude altitude,float pitch = 1.0f, float speedMultiplier = 1f)
	{
		var fireBatVelocity = transform.localScale.x * 25f * speedMultiplier;

		Firebat.Spawn(altitude == Altitude.High ? highAngle : lowAngle, fireBatVelocity, this);
		FirebatFirePillar.Spawn(this);

		var fireAudio = WeaverAudio.Play(Sounds.GrimmBatFire, transform.position, 1.0f, AudioChannel.Sound);
		Debug.Log("C");
		fireAudio.AudioSource.pitch = pitch;

		yield return new WaitForSeconds(0.3f);

		yield break;
	}


	IEnumerator TeleportIn(bool playSound = true)
	{
		if (invisible)
		{
			if (playSound)
			{
				WeaverAudio.Play(Sounds.GrimmTeleportIn, transform.position, 1.0f, AudioChannel.Sound);
			}

			PlayTeleportParticles();

			renderer.enabled = true;
			animator.enabled = true;


			yield return PlayAnimationTillDone("Tele In");
			collider.enabled = true;
			invisible = false;
		}
	}

	IEnumerator TeleportOut(bool playSound = true)
	{
		if (!invisible)
		{
			if (playSound)
			{
				WeaverAudio.Play(Sounds.GrimmTeleportOut, transform.position, 1.0f, AudioChannel.Sound);
			}
			collider.enabled = false;

			yield return PlayAnimationTillDone("Tele Out");

			renderer.enabled = false;
			animator.enabled = false;

			PlayTeleportParticles();

			invisible = true;
		}
	}

	void FacePlayer(bool textureFacesRight = true)
	{
		if (Player.Player1.transform.position.x <= transform.position.x)
		{
			renderer.flipX = textureFacesRight;
			FaceDirection = Direction.Left;
			FirebatSpawnpoint.transform.localPosition = new Vector3(fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
		else
		{
			renderer.flipX = !textureFacesRight;
			FaceDirection = Direction.Right;
			FirebatSpawnpoint.transform.localPosition = new Vector3(-fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
	}

	bool PlayerInFront()
	{
		if (FaceDirection == Direction.Left)
		{
			return Player.Player1.transform.position.x < transform.position.x;
		}
		else if (FaceDirection == Direction.Right)
		{
			return Player.Player1.transform.position.x > transform.position.x;
		}
		return false;
	}


	GrimmAttackMode GetRandomAttackMode()
	{
		var values = Enum.GetValues(typeof(GrimmAttackMode));
		return (GrimmAttackMode)values.GetValue(UnityEngine.Random.Range(1, values.GetLength(0) - 1));
	}

	void PlayTeleportParticles()
	{
		teleSmokeBack.Stop();
		teleSmokeBack.Play();

		teleSmokeFront.Stop();
		teleSmokeFront.Play();
	}

	float GetAngleTo(Vector3 Source, Vector3 Destination)
	{
		return Mathf.Atan2(Destination.y - Source.y, Destination.x - Source.x) * Mathf.Rad2Deg;
	}


	Vector2 Decelerate(Vector2 source, Vector2 deceleration)
	{
		if (deceleration.x > 0f)
		{
			source.x *= deceleration.x;
		}
		if (deceleration.y > 0f)
		{
			source.y *= deceleration.y;
		}
		return source;
	}
}
