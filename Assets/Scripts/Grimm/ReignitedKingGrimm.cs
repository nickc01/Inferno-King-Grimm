using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;
using WeaverCore.WeaverAssets;
using Random = UnityEngine.Random;


public enum Altitude
{
	High,
	Low
}

public enum GrimmDirection
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
	SpikesController spikeController;
	GrimmHealthManager healthManager;
	DamageHero damager;
	ExplosionEffects explosions;

	ParticleSystem DustScuttle;
	ParticleSystem DustGround;
	ParticleSystem DustUppercut;
	ParticleSystem UppercutExplosion;
	ParticleSystem BalloonParticles;
	ParticleSystem DeathExplosion;

	ParticleSystem DeathPuff;
	ParticleSystem SteamParticles;

	GameObject AudioScuttle;
	GameObject BalloonFireballShoot;
	GameObject BalloonCollider;
	GameObject DeathBurst;
	Animator ReformSprite;

	ParticleSystem teleSmokeBack;
	ParticleSystem teleSmokeFront;

	PolygonCollider2D Slash1;
	PolygonCollider2D Slash2;
	PolygonCollider2D Slash3;

	Coroutine BossRoutine;

	GameObject FirebatSpawnpoint;
	float fireBatSpawnpointX;

	bool invisible = true;

	bool continueToSlash = true;

	public bool Stunned { get; private set; }

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

	[Header("Grimm Spike Settings")]
	[SerializeField]
	float spikeWaitTime = 1.35f;

	[Header("Pillar Attack Settings")]
	[SerializeField]
	int pillarsToSpawn = 4;
	[SerializeField]
	float pillarSpawnRate = 0.75f;

	[Header("Ground Slash Settings")]
	[Space]
	[SerializeField]
	float evadeSpeed = 25f;
	[SerializeField]
	float groundSlashSpeed = 50f;
	[SerializeField]
	float uppercutHorizontalSpeed = 10f;
	[SerializeField]
	float uppercutVerticalSpeed = 45f;
	[SerializeField]
	float upperCutHeightLimit = 17.3f;
	[SerializeField]
	float upperCutTimeLimit = 0.35f;

	[Header("Bat Hit Settings")]
	[SerializeField]
	AudioSource BatAudioLoop;
	GrimmBatController BatController;
	[Header("Stun Settings")]
	[SerializeField]
	WeaverGameManager.TImeFreezePreset freezePreset;


	public static MainPrefabs Prefabs { get; private set; }

	public GrimmDirection FaceDirection { get; private set; }


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
		healthManager = GetComponent<GrimmHealthManager>();
		damager = GetComponent<DamageHero>();
		BatController = Instantiate(Prefabs.BatControllerPrefab);
		explosions = GetComponentInChildren<ExplosionEffects>(true);
		spikeController = Instantiate(Prefabs.spikeControllerPrefab,Prefabs.spikeControllerPrefab.transform.position,Quaternion.identity);

		FirebatSpawnpoint = transform.Find("Firebat SpawnPoint").gameObject;
		DustScuttle = transform.Find("Dust Scuttle").GetComponent<ParticleSystem>();
		DustGround = transform.Find("Dust Ground").GetComponent<ParticleSystem>();
		DustUppercut = transform.Find("Dust Uppercut").GetComponent<ParticleSystem>();
		UppercutExplosion = transform.Find("Uppercut Explosion").GetComponent<ParticleSystem>();
		BalloonParticles = transform.Find("Balloon Particles").GetComponent<ParticleSystem>();
		AudioScuttle = transform.Find("Audio Scuttle").gameObject;
		BalloonFireballShoot = transform.Find("Balloon Fireball Loop Audio").gameObject;
		BalloonCollider = transform.Find("Balloon Collider").gameObject;
		DeathBurst = transform.Find("Death Burst").gameObject;
		ReformSprite = transform.Find("Reform Sprite").GetComponent<Animator>();

		Slash1 = transform.Find("Slash1").GetComponent<PolygonCollider2D>();
		Slash2 = transform.Find("Slash2").GetComponent<PolygonCollider2D>();
		Slash3 = transform.Find("Slash3").GetComponent<PolygonCollider2D>();

		DeathPuff = transform.Find("Death Puff").GetComponent<ParticleSystem>();
		SteamParticles = transform.Find("Steam Pt").GetComponent<ParticleSystem>();
		DeathExplosion = transform.Find("Death Explode").GetComponent<ParticleSystem>();

		fireBatSpawnpointX = FirebatSpawnpoint.transform.localPosition.x;

		teleSmokeBack = transform.Find("tele_smoke_back").GetComponent<ParticleSystem>();
		teleSmokeFront = transform.Find("tele_smoke_front").GetComponent<ParticleSystem>();

		if (Core.LoadState == RunningState.Game)
		{
			receiver.ReceiveAllEventsFromName("WAKE");
			receiver.OnReceiveEvent += Wake;
		}
		else
		{
			StartCoroutine(Waiter(1,() => Wake("WAKE")));
		}

		Debugger.Log("Random Attack Mode = " + EnumUtilities.RandomEnumValue(GrimmAttackMode.Balloon,GrimmAttackMode.None));

		animator.enabled = false;
		renderer.enabled = false;
		collider.enabled = false;
	}

	/*IEnumerator animator.PlayAnimationTillDone(string animationStateName)
	{
		animator.Play(animationStateName);

		yield return null;

		//var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		yield return new WaitForSeconds(animator.GetCurrentAnimationTime());
	}

	float animator.GetCurrentAnimationTime()
	{
		var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		return stateInfo.length / stateInfo.speed;
	}

	void animator.PlayAnimation(string animationStateName)
	{
		animator.Play(animationStateName);
	}*/

	IEnumerator Waiter(float waitTime, Action OnDone)
	{
		yield return new WaitForSeconds(waitTime);
		OnDone();
	}

	//Called when the boss starts
	void Wake(string eventName)
	{
		if (eventName == "WAKE")
		{
			Debugger.Log("THE ENEMY HAS AWOKEN!!!");
			transform.position = CentralPosition;

			//BossRoutine = StartCoroutine(StopWhenStunned(MainBossControl()));
			BossRoutine = CoroutineUtilities.RunCoroutineWhile(this, MainBossControl(), () => !Stunned);
		}
	}

	IEnumerator MainBossControl()
	{
		yield return new WaitForSeconds(0.6f);
		while (true)
		{
			//yield return GroundSlashMove();
			//yield return PillarsMove();
			//yield return SpikesMove();
			//yield return AirDashMove();
			//yield return FirebatsMove();
			//yield return GroundSlashMove();
			rigidbody.velocity = Vector2.zero;
			//yield return BalloonMove();
			switch (EnumUtilities.RandomEnumValue(GrimmAttackMode.Balloon, GrimmAttackMode.None))
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
			}
		}
	}

	IEnumerator FirebatsMove()
	{
		if (invisible)
		{
			var playerPos = Player.Player1.transform.position;
			if (playerPos.x <= 88)
			{
				transform.position = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), groundY);
			}
			else
			{
				transform.position = new Vector3(UnityEngine.Random.Range(74.0f, 77.0f), groundY);
			}
		}

		FacePlayer(true);

		yield return TeleportIn();

		animator.PlayAnimation("Bat Cast Charge");


		//WeaverAudio.Play(Sounds.GrimmBeginBatCastVoice, transform.position);
		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);

		yield return new WaitForSeconds(0.2f);

		animator.PlayAnimation("Cast");

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

		animator.PlayAnimation("Cast Return");

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

		animator.PlayAnimation("Bat Cast Charge");

		yield return new WaitForSeconds(0.2f);

		animator.PlayAnimation("Cast");

		yield return new WaitForSeconds(0.3f);
		StartCoroutine(SendFirebat(Altitude.High, 1.0f));
		yield return SendFirebat(Altitude.Low, 1.0f);

		animator.PlayAnimation("Cast Return");

		yield return new WaitForSeconds(0.5f);

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator GroundSlashMove()
	{
		continueToSlash = true;
		float teleX = 0f;
		while (true)
		{
			var heroX = Player.Player1.transform.position.x;
			var teleAdder = Random.Range(7.5f, 9f);
			if (Random.value > 0.5f)
			{
				teleAdder = -teleAdder;
			}

			teleX = heroX + teleAdder;

			if (teleX > leftEdge && teleX < rightEdge)
			{
				break;
			}
		}

		transform.position = new Vector3(teleX, groundY, 0f);

		FacePlayer();

		yield return TeleportIn();

		var raycastDirection = FaceDirection == GrimmDirection.Right ? Vector2.right : Vector2.left;

		var hit = Physics2D.Raycast(transform.position, raycastDirection, 4, LayerMask.GetMask("Terrain"), 0, 0);

		Debug.DrawLine(transform.position, transform.position + ((Vector3)raycastDirection * 4f));

		//TODO - Make sure this works
		Debugger.Log("HIt = " + hit.transform);
		Debugger.Log("Hit Distance = " + hit.distance);
		if (hit.transform != null)
		{
			yield return GroundEvade();
		}
		else if (Random.value <= 0.2f)
		{
			yield return GroundEvade();
		}
		else if (!PlayerInFront())
		{
			yield return GroundEvade();
		}
		if (!continueToSlash)
		{
			yield break;
		}

		FacePlayer();

		WeaverAudio.Play(Sounds.SlashAntic, transform.position);

		animator.PlayAnimation("Slash Antic");

		VoicePlayer.Play(Sounds.SlashAnticVoice);

		yield return new WaitForSeconds(0.5f);

		var xScale = transform.lossyScale.x;

		WeaverAudio.Play(Sounds.GroundSlashAttack, transform.position);

		var speed = xScale * groundSlashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			speed = -speed;
		}

		rigidbody.velocity = new Vector2(speed, 0f);

		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform,leftEdge,rightEdge), () => !Stunned);

		DustGround.Play();

		Slash1.enabled = true;

		yield return animator.PlayAnimationTillDone("Slash 1");

		Slash1.enabled = false;
		Slash2.enabled = true;

		yield return animator.PlayAnimationTillDone("Slash 2");

		Slash2.enabled = false;
		Slash3.enabled = true;

		yield return animator.PlayAnimationTillDone("Slash 3");


		Slash3.enabled = false;
		DustGround.Stop();
		animator.PlayAnimation("Slash Recover");

		//This ensures that the current animation is playing
		yield return null;

		yield return CoroutineUtilities.RunForPeriod(animator.GetCurrentAnimationTime(), () =>
		{
			rigidbody.velocity = VectorUtilities.Decelerate(rigidbody.velocity, new Vector2(0.65f,0.65f));
		});

		rigidbody.velocity = Vector2.zero;
		StopCoroutine(lockRoutine);

		yield return UpperCut();

		yield break;
	}

	IEnumerator HorizontalLock(Transform transform, float leftEdge, float rightEdge)
	{
		while (true)
		{
			if (transform.position.x < leftEdge)
			{
				transform.position = transform.position.With(leftEdge);
			}

			if (transform.position.x > rightEdge)
			{
				transform.position = transform.position.With(rightEdge);
			}
			yield return null;
		}
	}

	IEnumerator UpperCut()
	{
		if (invisible)
		{
			FacePlayer();
			yield return TeleportIn();
		}
		rigidbody.velocity = Vector2.zero;

		//animator.PlayAnimation("Uppercut Antic");

		VoicePlayer.Play(Sounds.GrimmUppercutAttackVoice);

		//yield return new WaitForSeconds(0.45f);

		yield return animator.PlayAnimationTillDone("Uppercut Antic");

		DustUppercut.Play();

		animator.PlayAnimation("Uppercut");

		var sf = WeaverAudio.Play(Sounds.UpperCutSoundEffect, transform.position);
		sf.AudioSource.pitch = 1.1f;

		var xScale = transform.lossyScale.x;

		var horizontalSpeed = xScale * uppercutHorizontalSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			horizontalSpeed = -horizontalSpeed;
		}

		rigidbody.velocity = new Vector2(horizontalSpeed,uppercutVerticalSpeed);
		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, leftEdge, rightEdge), () => !Stunned);

		//This function will wait until either the time is up, or the y position is no longer less than or equal to the uppercut height limit
		yield return CoroutineUtilities.RunForPeriod(0.35f, () => transform.position.y <= upperCutHeightLimit);

		transform.position = transform.position.With(y: upperCutHeightLimit);//new Vector3(transform.position.x,upperCutHeightLimit,transform.position.z);

		StopCoroutine(lockRoutine);

		//TODO -- Activate Red Flash 1

		DustUppercut.Stop();

		var explodeSF = WeaverAudio.Play(Sounds.UpperCutExplodeEffect, transform.position);
		explodeSF.AudioSource.pitch = 1.1f;

		rigidbody.velocity = Vector2.zero;

		//TODO Shake Camera - AverageShake

		UppercutExplosion.Play();

		var fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(0f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(12f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(-12f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(24f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(-24f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(36f, 1f);
		fireBall = Instantiate(Prefabs.UppercutFireball, transform.position, Quaternion.identity);
		fireBall.velocity = new Vector2(-36f, 1f);

		yield return animator.PlayAnimationTillDone("Uppercut End");

		invisible = true;
		collider.enabled = false;
		renderer.enabled = false;
		animator.enabled = false;

		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(0.6f);

	}

	IEnumerator GroundEvade()
	{
		continueToSlash = true;
		FacePlayer();

		//WeaverAudio.Play(Sounds.SlashAntic,transform.position);

		VoicePlayer.Play(Sounds.GrimmEvadeVoice);

		yield return animator.PlayAnimationTillDone("Evade Antic");

		var xScale = transform.lossyScale.x;

		WeaverAudio.Play(Sounds.EvadeSoundEffect,transform.position);

		var speed = xScale * evadeSpeed;

		if (FaceDirection == GrimmDirection.Right)
		{
			speed = -speed;
		}

		rigidbody.velocity = new Vector2(speed,0f);

		animator.PlayAnimation("Evade");

		//DustScuttle.SetActive(true);
		DustScuttle.Play();
		AudioScuttle.SetActive(true);

		//yield return new WaitForSeconds(0.225f);
		float waitTimer = 0f;
		float waitTime = 0.225f;

		bool horizontal = true;

		do
		{
			yield return null;
			waitTimer += Time.deltaTime;
			if (horizontal && (transform.position.x <= leftEdge || transform.position.x >= rightEdge))
			{
				rigidbody.velocity = Vector2.zero;//VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				horizontal = false;
			}
		} while (waitTimer < waitTime);

		//DustScuttle.SetActive(false);
		DustScuttle.Stop();
		AudioScuttle.SetActive(false);

		rigidbody.velocity = new Vector2(0f, 0f);

		yield return animator.PlayAnimationTillDone("Evade End");

		if (Mathf.Abs(transform.position.x - Player.Player1.transform.position.x) > 7f)
		{
			continueToSlash = false;
			yield return FirebatsMove();
		}
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

		Debugger.Log("B");

		transform.position = new Vector3(teleX, 15f, 0f);

		FacePlayer();

		yield return TeleportIn();

		Debugger.Log("Angle To Player = " + transform.position.GetAngleBetween(Player.Player1.transform.position));

		var velocityAngle = Mathf.Clamp(transform.position.GetAngleBetween(Player.Player1.transform.position),-135f,-45f);
		var spriteAngle = velocityAngle + 90f;

		Debugger.Log("Angle = " + velocityAngle);
		//WeaverAudio.Play(Sounds.AirDashAntic, transform.position, 1, AudioChannel.Sound);

		//VoicePlayer.Play();

		yield return animator.PlayAnimationTillDone("Air Dash Antic");

		animator.PlayAnimation("Air Dash");


		rigidbody.velocity = VectorUtilities.AngleToVector(velocityAngle * Mathf.Deg2Rad, airDashSpeed);
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
				rigidbody.velocity = VectorUtilities.AngleToVector(- 90f * Mathf.Deg2Rad, airDashSpeed);
				transform.eulerAngles = Vector3.zero;
				vertical = true;
			}
		} while (transform.position.y > groundY);


		rigidbody.velocity = Vector2.zero;

		transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

		animator.PlayAnimation("Ground Dash Antic");

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

		if (FaceDirection == GrimmDirection.Left)
		{
			groundEffect.GetComponent<SpriteRenderer>().flipX = true;
			groundEffect.transform.position -= Prefabs.GroundDashEffect.transform.position * 2f;
			dashSpeed = -dashSpeed;
		}

		rigidbody.velocity = new Vector2(dashSpeed,0f);

		animator.PlayAnimation("G Dash");

		DashSpike.enabled = true;

		var dustEffect = Instantiate(Prefabs.DustGroundEffect, transform.position + Prefabs.DustGroundEffect.transform.position, Prefabs.DustGroundEffect.transform.rotation).GetComponent<ParticleSystem>();
		var mainParticles = dustEffect.main;

		mainParticles.startLifetime = groundDashTime;
		Destroy(dustEffect.gameObject, groundDashTime + 0.5f);


		//dustEffect.main.startLifetime = groundDashTime;

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
			if (horizontal && (transform.position.x <= leftEdge || transform.position.x >= rightEdge))
			{
				rigidbody.velocity = Vector2.zero;//VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				horizontal = false;
			}
		} while (waitTimer < groundDashTime);

		animator.PlayAnimation("Ground Dash Antic");

		waitTimer = 0f;

		DashSpike.enabled = false;

		dustEffect.Stop();

		do
		{
			yield return null;
			rigidbody.velocity = VectorUtilities.Decelerate(rigidbody.velocity, new Vector2(0.75f,float.NaN));
			waitTimer += Time.deltaTime;
		} while (waitTimer < 0.33f);

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator SpikesMove()
	{
		spikeController.Play();
		yield return new WaitForSeconds(spikeWaitTime);

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator PillarsMove()
	{
		float teleX = 0f;

		while (true)
		{
			var playerX = Player.Player1.transform.position.x;
			var selfX = transform.position.x;

			var teleAdder = Random.Range(12f, 13f);

			if (Random.value > 0.5f)
			{
				teleAdder = -teleAdder;
			}

			teleX = playerX + teleAdder;

			if (teleX >= leftEdge && teleX <= rightEdge)
			{
				break;
			}
			yield return null;
		}

		transform.position = new Vector3(teleX,13.1f,0f);

		FacePlayer();

		yield return TeleportIn();

		animator.PlayAnimation("Pillar Idle");

		WeaverAudio.Play(Sounds.PillarAntic, transform.position);

		VoicePlayer.Play(Sounds.PillarCastVoice);

		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < pillarsToSpawn; i++)
		{
			Instantiate(Prefabs.FlamePillarPrefab,Player.Player1.transform.position + Prefabs.FlamePillarPrefab.transform.position,Quaternion.identity);

			yield return new WaitForSeconds(0.75f);
		}
		yield return new WaitForSeconds(0.25f);
		yield return TeleportOut();
		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator BalloonMove()
	{
		//transform.position = new Vector3(85.77f,13.5f,transform.position.z);
		transform.position = transform.position.With(x: 85.77f, y: 13.5f);

		while (Vector3.Distance(transform.position,Player.Player1.transform.position) < 6f)
		{
			yield return null;
		}

		FacePlayer();

		yield return TeleportIn();

		//TODO - EnemyKillShake

		yield return animator.PlayAnimationTillDone("Balloon Antic");

		//TODO - BROADCAST THE CROWD GASP EVENT

		//EventReceiver.BroadcastEvent("CROWD GASP");

		//TODO Broadcast HEART HALFWAY EVENT

		BalloonFireballShoot.SetActive(true);

		WeaverAudio.Play(Sounds.GrimmScream, transform.position);
		WeaverAudio.Play(Sounds.InflateSoundEffect, transform.position);

		//TODO Broadcast CROWD CLAP DELAY EVENT

		var angle = Random.Range(0f, 360f);

		animator.PlayAnimation("Balloon");

		FacePlayer();

		BalloonCollider.SetActive(true);

		BalloonParticles.Play();

		healthManager.Invincible = true;

		int amountOfWaves = 12;

		int ballCounter = 0;
		int waveCounter = 0;

		var ballSpawn = transform.position.With(z: 0.001f);

		for (int i = 0; i < amountOfWaves; i++)
		{
			yield return new WaitForSeconds(0.6f);
			ballCounter++;
			waveCounter++;
			/*if (ballCounter == 4)
			{
				ballCounter = 0;
				GrimmBall.Spawn(ballSpawn, -7.6f, 10f, 4.5f);
				GrimmBall.Spawn(ballSpawn, -7.6f, -10f, -4.5f);

				GrimmBall.Spawn(ballSpawn, -7.6f, -5.6f, 5f);
				GrimmBall.Spawn(ballSpawn, -7.6f, -10f, -5f);
			}
			else
			{
				GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, 10f, 4.5f);
				GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, -10f, -4.5f);

				GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, 10f, 4.5f);
				GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, -10f, -4.5f);
			}*/

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, -10f, -4.5f);

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, -10f, -4.5f);

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 3f : 5f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 3f : 5f, -10f, -4.5f);
		}

		yield return new WaitForSeconds(0.75f);

		BalloonParticles.Stop();

		healthManager.Invincible = false;

		BalloonCollider.SetActive(false);

		//TODO - Broadcast CROWD IDLE EVENT

		BalloonFireballShoot.SetActive(false);

		WeaverAudio.Play(Sounds.DeflateSoundEffect, transform.position);

		yield return TeleportOut();

		yield return new WaitForSeconds(0.6f);
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

			WeaverEvents.BroadcastEvent("EnemyKillShake");

			yield return animator.PlayAnimationTillDone("Tele In");
			collider.enabled = true;
			invisible = false;
		}
	}

	public void MakeVisible(bool visible = true)
	{
		invisible = !visible;
		renderer.enabled = visible;
		animator.enabled = visible;
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

			yield return animator.PlayAnimationTillDone("Tele Out");

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
			FaceDirection = GrimmDirection.Left;
			FirebatSpawnpoint.transform.localPosition = new Vector3(fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
		else
		{
			renderer.flipX = !textureFacesRight;
			FaceDirection = GrimmDirection.Right;
			FirebatSpawnpoint.transform.localPosition = new Vector3(-fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
	}

	bool PlayerInFront()
	{
		if (FaceDirection == GrimmDirection.Left)
		{
			return Player.Player1.transform.position.x < transform.position.x;
		}
		else if (FaceDirection == GrimmDirection.Right)
		{
			return Player.Player1.transform.position.x > transform.position.x;
		}
		return false;
	}


	/*GrimmAttackMode GetRandomAttackMode()
	{
		var values = Enum.GetValues(typeof(GrimmAttackMode));
		return (GrimmAttackMode)values.GetValue(UnityEngine.Random.Range(1, values.GetLength(0) - 1));
	}*/

	void PlayTeleportParticles()
	{
		teleSmokeBack.Stop();
		teleSmokeBack.Play();

		teleSmokeFront.Stop();
		teleSmokeFront.Play();
	}

	/*float GetAngleTo(Vector3 Source, Vector3 Destination)
	{
		return Mathf.Atan2(Destination.y - Source.y, Destination.x - Source.x) * Mathf.Rad2Deg;
	}*/

	/*IEnumerator RunForPeriod(float time, Action action)
	{
		yield return RunForPeriod(time, t => action());
	}

	IEnumerator RunForPeriod(float time, Action<float> action)
	{
		yield return RunForPeriod(time, t =>
		{
			action(t);
			return true;
		});
	}

	IEnumerator RunForPeriod(float time, Func<bool> action)
	{
		yield return RunForPeriod(time, t => action());
	}

	IEnumerator RunForPeriod(float time, Func<float,bool> action)
	{
		float timer = 0;
		while (timer < time)
		{
			yield return null;
			timer += Time.deltaTime;
			if (!action(timer))
			{
				timer = time;
			}
		}
	}

	IEnumerable RunTillFalse(Func<bool> action)
	{
		yield return RunTillFalse(t => action());
	}

	IEnumerable RunTillFalse(Func<float,bool> action)
	{
		float timer = 0;
		do
		{
			yield return null;
			timer += Time.deltaTime;
		} while (action(timer));
	}*/


	/*Vector2 Decelerate(Vector2 source, Vector2 deceleration)
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
	}*/

	public void ReachedHealthStage(int stage)
	{
		Debugger.Log("AT HEALTH STAGE = " + stage);
		//StartCoroutine(Stun());
	}

	IEnumerator Stun()
	{
		if (Stunned == true)
		{
			yield break;
		}
		Stunned = true;

		collider.enabled = false;

		Debugger.Log("Boss Routine = " + BossRoutine);
		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}

		Slash1.enabled = false;
		Slash2.enabled = false;
		Slash3.enabled = false;
		DashSpike.enabled = false;
		transform.rotation = Quaternion.identity;
		DustGround.Stop();
		BalloonParticles.Stop();
		DustScuttle.Stop();
		DustUppercut.Stop();
		damager.enabled = false;
		healthManager.Invincible = true;
		rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
		rigidbody.velocity = Vector2.zero;

		//TODO - Make Camera Shake

		WeaverEvents.BroadcastEvent("EnemyKillShake");

		WeaverGameManager.FreezeGameTime(freezePreset);

		//TODO - Cause the game to freeze temporarily

		yield return null;

		Instantiate(Prefabs.StunEffect, transform.position, Quaternion.identity);

		rigidbody.velocity = Vector2.zero;

		yield return animator.PlayAnimationTillDone("Explode Antic");

		VoicePlayer.Play(Sounds.GrimmScream);

		//TODO Broadcast Event - CROWD STILL
		WeaverAudio.Play(Sounds.GrimmBatExplosion, transform.position);

		explosions.Play();

		BatController.SendOut(this);

		//TODO - Broadcast Event - CROWD STILL
		WeaverEvents.BroadcastEvent("CROWD STILL");

		BatAudioLoop.gameObject.SetActive(true);

		yield return animator.PlayAnimationTillDone("Explode");

		MakeVisible(false);

		yield return new WaitForSeconds(2f);

		BatController.BringIn();

		//TODO - Shake Camera

		BatAudioLoop.gameObject.SetActive(false);

		yield return new WaitForSeconds(0.4f);

		var oldColor = renderer.color;

		renderer.color = Color.black;

		//renderer.enabled = true;
		//MakeVisible();

		WeaverAudio.Play(Sounds.GrimmTeleportOut, transform.position);
		WeaverAudio.Play(Sounds.GrimmBatsReform, transform.position);

		ReformSprite.gameObject.SetActive(true);

		yield return ReformSprite.PlayAnimationTillDone("Default");
		yield return ReformSprite.PlayAnimationTillDone("Default Loop");
		//yield return animator.PlayAnimationTillDone("Reform");

		ReformSprite.gameObject.SetActive(false);

		PlayTeleportParticles();
		//WeaverAudio.Play(Sounds.GrimmTeleportOut, transform.position, 1.0f, AudioChannel.Sound);

		//MakeVisible(false);

		renderer.color = oldColor;

		//TODO - Broadcast event CROWD CLAP
		WeaverEvents.BroadcastEvent("CROWD CLAP");

		//TODO - Make Camera Shake - AverageShake

		yield return new WaitForSeconds(0.6f);

		Stunned = false;
		rigidbody.constraints = RigidbodyConstraints2D.None;
		damager.enabled = true;
		healthManager.Invincible = false;

		//BossRoutine = StartCoroutine(StopWhenStunned(MainBossControl()));
		BossRoutine = CoroutineUtilities.RunCoroutineWhile(this, MainBossControl(), () => !Stunned);

		yield break;
	}

	public void OnDeath()
	{
		if (Stunned == true)
		{
			return;
		}
		Stunned = true;

		collider.enabled = false;
		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}
		Debugger.Log("Boss Dead");
		BalloonFireballShoot.SetActive(false);
		healthManager.Invincible = true;

		Slash1.enabled = false;
		Slash2.enabled = false;
		Slash3.enabled = false;
		DashSpike.enabled = false;
		transform.rotation = Quaternion.identity;
		DustGround.Stop();
		BalloonParticles.Stop();
		DustScuttle.Stop();
		DustUppercut.Stop();
		damager.enabled = false;
		healthManager.Invincible = true;
		rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
		rigidbody.velocity = Vector2.zero;
		transform.eulerAngles = Vector3.zero;

		StartCoroutine(DeathRoutine());
	}

	struct SnapshotHolder
	{
		public AudioMixerSnapshot snapshot;
	}

	IEnumerator DeathRoutine()
	{
		var scream = WeaverAudio.Play(Sounds.GrimmScream, transform.position, autoPlay: false);
		scream.AudioSource.PlayDelayed(0.5f);

		WeaverEvents.BroadcastEvent("HEARTBEAT STOP");

		//TODO : Shake Camera AverageShake

		var swordDeath = WeaverAudio.Play(AudioAssets.EnemyDeathBySword, transform.position);
		swordDeath.AudioSource.pitch = 0.75f;

		var enemyDamage = WeaverAudio.Play(AudioAssets.DamageEnemy, transform.position);
		enemyDamage.AudioSource.pitch = 0.75f;

		var endingTune = WeaverAudio.Play(Sounds.EndingTune, transform.position, autoPlay: false);
		endingTune.AudioSource.PlayDelayed(0.3f);

		TransformUtilities.SpawnRandomObjects(EffectAssets.GhostSlash1Prefab, transform.position, 8, 8, 2f, 35f, 0f, 360f);
		TransformUtilities.SpawnRandomObjects(EffectAssets.GhostSlash2Prefab, transform.position, 2, 3, 2f, 35f, 0f, 360f);

		DeathBurst.SetActive(true);

		animator.PlayAnimation("Death Stun");

		FacePlayer(false);

		rigidbody.velocity = Vector2.zero;

		//TODO Set Audio Snapsho
		var gameMixer = WeaverAudio.MainMixer;
		if (gameMixer != null && ImplFinder.State == RunningState.Game)
		{
			SnapshotHolder snapshot = JsonUtility.FromJson<SnapshotHolder>("{\"snapshot\":{ \"m_FileID\":" + 15948 + ",\"m_PathID\":" + 0 + "}}");

			if (snapshot.snapshot != null)
			{
				snapshot.snapshot.TransitionTo(1f);
			}
		}

		yield return new WaitForSeconds(1f);

		//TODO - HIDE HUD

		var jitterRoutine = StartCoroutine(TransformUtilities.JitterObject(gameObject,new Vector3(0.2f,0.2f,0f)));

		WeaverEvents.BroadcastEvent("HEARTBEAT FAST");

		WeaverAudio.Play(AudioAssets.BossFinalHit, transform.position);
		WeaverAudio.Play(AudioAssets.BossGushing, transform.position);

		WeaverEvents.BroadcastEvent("BigShake"); //???

		DeathPuff.Play();
		SteamParticles.Play();

		//TODO - SHake Camera - BigShake

		float emitRate = 50f;
		float emitSpeed = 5f;

		WeaverEvents.BroadcastEvent("CROWD GASP");

		for (float t = 0; t < 3f; t += Time.deltaTime)
		{
			emitRate += 2f;
			emitSpeed += 0.5f;

			emitSpeed = Mathf.Clamp(emitSpeed, 0f, 110f);

			var particleModule = DeathPuff.emission;
			var mainModule = DeathPuff.main;

			particleModule.rateOverTime = emitRate;
			mainModule.startSpeed = emitSpeed;

			yield return null;

		}

		StopCoroutine(jitterRoutine);

		DeathPuff.Stop();
		SteamParticles.Stop();

		WeaverAudio.Play(Sounds.UpperCutExplodeEffect, transform.position);

		WeaverEvents.BroadcastEvent("CROWD STILL");

		WeaverEvents.BroadcastEvent("BigShake");

		MakeVisible(false);

		explosions.Play();

		DeathExplosion.Play();

		WeaverAudio.Play(AudioAssets.BossExplosionUninfected, transform.position);

		WeaverEvents.BroadcastEvent("GRIMM DEFEATED");

		//EndBossBattle(2f);

		yield break;
	}

	/*IEnumerator JitterObject(GameObject obj, Vector3 amount)
	{
		Vector3 startPosition = obj.transform.position;

		while (true)
		{
			yield return null;
			transform.position = new Vector3(startPosition.x + Random.Range(-amount.x,amount.x),startPosition.y + Random.Range(-amount.y,amount.y),startPosition.z + Random.Range(-amount.z,amount.z));
		}
	}

	public void SpawnRandomObjects(GameObject obj, Vector3 spawnPoint, int spawnMin, int spawnMax, float minSpeed,float maxSpeed, float angleMin, float angleMax, Vector2 originOffset = default(Vector2))
	{
		int spawnNum = Random.Range(spawnMin, spawnMax + 1);
		float speedNum = Random.Range(minSpeed, maxSpeed);
		float angleNum = Random.Range(angleMin, angleMax);

		for (int i = 0; i < spawnNum; i++)
		{
			var instance = Instantiate(obj, new Vector3(spawnPoint.x + Random.Range(-originOffset.x, originOffset.x), spawnPoint.y + Random.Range(-originOffset.y, originOffset.y), spawnPoint.z),Quaternion.identity);
			var rigid = instance.GetComponent<Rigidbody2D>();
			if (rigid != null)
			{
				rigid.velocity = new Vector2(Mathf.Cos(angleNum) * Mathf.Deg2Rad,Mathf.Sin(angleNum) * Mathf.Deg2Rad);
			}
		}
	}*/
}
