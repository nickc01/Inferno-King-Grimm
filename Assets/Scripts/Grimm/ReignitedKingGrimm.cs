using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;
using WeaverCore.WeaverAssets;


[RequireComponent(typeof(DamageHero))]
public class ReignitedKingGrimm : BossReplacement
{
	private List<GrimmMove> AllMoves;
	private BalloonMove balloonMove;

	bool balloonMoveNext = false;

	private readonly List<GrimmMove> possibleMoveSets = new List<GrimmMove>();

	public GrimmAttackMode CurrentAttackMode { get; private set; }
	public GrimmMove CurrentMove { get; private set; }
	public Animator GrimmAnimator { get; private set; }
	public BoxCollider2D GrimmCollider { get; private set; }
	public GrimmSounds Sounds { get; private set; }
	public WeaverAudioPlayer VoicePlayer { get; private set; }
	public GrimmHealthManager GrimmHealth { get; private set; }
	public Rigidbody2D GrimmRigidbody { get; private set; }
	public GrimmDirection FaceDirection { get; private set; }
	public int BossStage { get; private set; }
	public bool Stunned { get; private set; }
	public Vector2 Velocity
	{
		get
		{
			return GrimmRigidbody.velocity;
		}
		set
		{
			if (Stunned)
			{
				GrimmRigidbody.velocity = default(Vector2);
			}
			else
			{
				GrimmRigidbody.velocity = value;
			}
		}
	}

	private EventReceiver receiver;
	private SpriteRenderer spriteRenderer;
	private DamageHero damager;
	private ExplosionEffects explosions;
	private ParticleSystem DeathExplosion;
	private ParticleSystem DeathPuff;
	private ParticleSystem SteamParticles;
	private GameObject DeathBurst;
	private Animator ReformSprite;
	private ParticleSystem teleSmokeBack;
	private ParticleSystem teleSmokeFront;
	private Coroutine BossRoutine;
	private GameObject FirebatSpawnpoint;
	private GrimmMove previousMove;
	private float fireBatSpawnpointX;
	private bool invisible = true;

	[Space]
	[Space]
	[Header("Prefabs")]
	[SerializeField]
	private MainPrefabs prefabs;


	[Header("Arena Settings")]
	[SerializeField]
	private Vector3 StartingPosition = new Vector3(85.77f, 13.5f, 0.0f);
	[SerializeField]
	private readonly float groundY = 8.3f;
	[SerializeField]
	private readonly float leftEdge = 69.7f;
	[SerializeField]
	private readonly float rightEdge = 102.23f;

	[Header("Bat Hit Settings")]
	[SerializeField]
	private AudioSource BatAudioLoop;
	private GrimmBatController BatController;
	[Header("Stun Settings")]
	[SerializeField]
	private readonly WeaverGameManager.TimeFreezePreset freezePreset;


	public bool Invisible
	{
		get
		{
			return invisible;
		}
		set
		{
			invisible = value;
			spriteRenderer.enabled = !invisible;
			GrimmAnimator.enabled = !invisible;
		}
	}

	public float GroundY
	{
		get
		{
			return groundY;
		}
	}

	public float LeftEdge
	{
		get
		{
			return leftEdge;
		}
	}

	public float RightEdge
	{
		get
		{
			return rightEdge;
		}
	}

	public MainPrefabs Prefabs
	{
		get
		{
			return prefabs;
		}
	}

	private void Awake()
	{
		MainPrefabs.Instance = prefabs;
	}

	// Use this for initialization
	private void Start()
	{
		BossStage = 1;
		AllMoves = GetComponents<GrimmMove>().ToList();
		balloonMove = GetComponent<BalloonMove>();

		Debugger.Log("ENEMY HAS STARTED");

		if ((receiver = GetComponent<EventReceiver>()) == null)
		{
			receiver = gameObject.AddComponent<EventReceiver>();
		}

		GrimmRigidbody = GetComponent<Rigidbody2D>();
		VoicePlayer = GetComponent<WeaverAudioPlayer>();
		Sounds = GetComponentInChildren<GrimmSounds>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		GrimmAnimator = GetComponent<Animator>();
		GrimmCollider = GetComponent<BoxCollider2D>();
		GrimmHealth = GetComponent<GrimmHealthManager>();
		damager = GetComponent<DamageHero>();
		BatController = Instantiate(Prefabs.BatControllerPrefab);
		explosions = GetComponentInChildren<ExplosionEffects>(true);

		FirebatSpawnpoint = transform.Find("Firebat SpawnPoint").gameObject;
		DeathBurst = transform.Find("Death Burst").gameObject;
		ReformSprite = transform.Find("Reform Sprite").GetComponent<Animator>();

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
			StartCoroutine(Waiter(1, () => Wake("WAKE",gameObject)));
		}
		GrimmAnimator.enabled = false;
		spriteRenderer.enabled = false;
		GrimmCollider.enabled = false;

		var quarterHealth = GrimmHealth.Health / 4;
		var thirdHealth = GrimmHealth.Health / 3;

		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - quarterHealth,DoBalloonMove);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 2),DoBalloonMove);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 3), DoBalloonMove);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (thirdHealth), Stun);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (thirdHealth * 2), Stun);

	}

	public GrimmMove GetRandomMove()
	{
		List<GrimmMove> ValidMoves = AllMoves.Where(m => m.ExcludeFromRandomizer == false).ToList();

		GrimmMove selectedMove = ValidMoves.GetRandomElement();

		while (ValidMoves.Count > 1 && selectedMove == previousMove)
		{
			selectedMove = ValidMoves.GetRandomElement();
		}
		previousMove = selectedMove;

		return selectedMove;
	}

	private IEnumerator Waiter(float waitTime, Action OnDone)
	{
		yield return new WaitForSeconds(waitTime);
		OnDone();
	}

	//Called when the boss starts
	private void Wake(string eventName, GameObject source)
	{
		//Debugger.Log("Event Received for RKG = " + eventName);
		//Debugger.Log("Event Source for RKG = " + source);

		//Debugger.Log("Waking Trace = " + new System.Diagnostics.StackTrace(true));
		if (eventName == "WAKE" && (source == gameObject || source.name.Contains("Grimm Control")))
		{
			Debugger.Log("THE ENEMY HAS AWOKEN!!!");
			transform.position = StartingPosition;

			//BossRoutine = StartCoroutine(StopWhenStunned(MainBossControl()));
			BossRoutine = CoroutineUtilities.RunCoroutineWhile(this, MainBossControl(), () => !Stunned);
		}
	}

	private IEnumerator MainBossControl()
	{
		Debugger.Log("DOING MAIN BOSS CONTROL");
		yield return new WaitForSeconds(0.6f);
		while (true)
		{
			GrimmMove NextMove = null;
			if (balloonMoveNext)
			{
				yield return balloonMove.DoMove();
				balloonMoveNext = false;
			}
			else
			{
				CurrentMove = GetRandomMove();

				Debugger.Log("Current Move = " + CurrentMove.GetType());
				/*do
				{
					NextMove = GetRandomMove();
				} while (CurrentMove == NextMove);
				CurrentMove = NextMove;*/

				yield return CurrentMove.DoMove();
			}
		}
	}

	public IEnumerator TeleportIn(bool playSound = true)
	{
		if (Invisible)
		{
			if (playSound)
			{
				WeaverAudio.Play(Sounds.GrimmTeleportIn, transform.position, 1.0f, AudioChannel.Sound);
			}

			PlayTeleportParticles();

			Invisible = false;

			//WeaverEvents.BroadcastEvent("EnemyKillShake");
			WeaverCam.Instance.Shaker.Shake(ShakeType.EnemyKillShake);

			yield return GrimmAnimator.PlayAnimationTillDone("Tele In");
			GrimmCollider.enabled = true;
		}
	}

	public IEnumerator TeleportOut(bool playSound = true)
	{
		if (!invisible)
		{
			if (playSound)
			{
				WeaverAudio.Play(Sounds.GrimmTeleportOut, transform.position, 1.0f, AudioChannel.Sound);
			}
			GrimmCollider.enabled = false;

			yield return GrimmAnimator.PlayAnimationTillDone("Tele Out");

			Invisible = true;

			PlayTeleportParticles();
		}
	}

	public void FacePlayer(Vector3 lookingPosition, bool textureFacesRight = true)
	{
		if (Player.Player1.transform.position.x <= lookingPosition.x)
		{
			spriteRenderer.flipX = textureFacesRight;
			FaceDirection = GrimmDirection.Left;
			FirebatSpawnpoint.transform.localPosition = new Vector3(fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
		else
		{
			spriteRenderer.flipX = !textureFacesRight;
			FaceDirection = GrimmDirection.Right;
			FirebatSpawnpoint.transform.localPosition = new Vector3(-fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
	}

	public void FacePlayer(bool textureFacesRight = true)
	{
		FacePlayer(transform.position, textureFacesRight);
		/*if (Player.Player1.transform.position.x <= transform.position.x)
		{
			spriteRenderer.flipX = textureFacesRight;
			FaceDirection = GrimmDirection.Left;
			FirebatSpawnpoint.transform.localPosition = new Vector3(fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}
		else
		{
			spriteRenderer.flipX = !textureFacesRight;
			FaceDirection = GrimmDirection.Right;
			FirebatSpawnpoint.transform.localPosition = new Vector3(-fireBatSpawnpointX, FirebatSpawnpoint.transform.localPosition.y, FirebatSpawnpoint.transform.localPosition.z);
		}*/
	}

	private void PlayTeleportParticles()
	{
		teleSmokeBack.Stop();
		teleSmokeBack.Play();

		teleSmokeFront.Stop();
		teleSmokeFront.Play();
	}

	public void Stun()
	{
		//Debugger.Log("AT HEALTH STAGE = " + stage);
		BossStage++;
		StartCoroutine(StunRoutine());
	}

	public void DoBalloonMove()
	{
		balloonMoveNext = true;
	}

	private IEnumerator StunRoutine()
	{
		if (Stunned == true)
		{
			yield break;
		}
		Stunned = true;

		GrimmCollider.enabled = false;

		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}

		transform.rotation = Quaternion.identity;
		damager.enabled = false;
		GrimmHealth.Invincible = true;
		GrimmRigidbody.velocity = Vector2.zero;
		GrimmRigidbody.Sleep();

		CurrentMove.OnStun();

		//TODO - Make Camera Shake

		WeaverEvents.BroadcastEvent("EnemyKillShake");

		WeaverGameManager.FreezeGameTime(freezePreset);

		//TODO - Cause the game to freeze temporarily

		yield return null;

		Instantiate(Prefabs.StunEffect, transform.position, Quaternion.identity);

		GrimmRigidbody.velocity = Vector2.zero;

		yield return GrimmAnimator.PlayAnimationTillDone("Explode Antic");

		VoicePlayer.Play(Sounds.GrimmScream);

		//TODO Broadcast Event - CROWD STILL
		WeaverAudio.Play(Sounds.GrimmBatExplosion, transform.position);

		explosions.Play();

		BatController.SendOut(this);

		//TODO - Broadcast Event - CROWD STILL
		WeaverEvents.BroadcastEvent("CROWD STILL");

		BatAudioLoop.gameObject.SetActive(true);

		yield return GrimmAnimator.PlayAnimationTillDone("Explode");

		//MakeVisible(false);
		Invisible = true;

		yield return new WaitForSeconds(2f);

		BatController.BringIn();

		//TODO - Shake Camera

		BatAudioLoop.gameObject.SetActive(false);

		yield return new WaitForSeconds(0.4f);

		Color oldColor = spriteRenderer.color;

		spriteRenderer.color = Color.black;

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

		spriteRenderer.color = oldColor;

		//TODO - Broadcast event CROWD CLAP
		WeaverEvents.BroadcastEvent("CROWD CLAP");

		//TODO - Make Camera Shake - AverageShake

		yield return new WaitForSeconds(0.6f);

		Stunned = false;
		damager.enabled = true;
		GrimmHealth.Invincible = false;

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

		GrimmCollider.enabled = false;
		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}
		Debugger.Log("Boss Dead");

		//Slash1.enabled = false;
		//Slash2.enabled = false;
		//Slash3.enabled = false;
		//DashSpike.enabled = false;
		transform.rotation = Quaternion.identity;
		//DustGround.Stop();
		//DustScuttle.Stop();
		//DustUppercut.Stop();
		damager.enabled = false;
		GrimmHealth.Invincible = true;
		GrimmRigidbody.velocity = Vector2.zero;
		GrimmRigidbody.Sleep();

		CurrentMove.OnDeath();

		StartCoroutine(DeathRoutine());
	}

	private struct SnapshotHolder
	{
		public AudioMixerSnapshot snapshot;
	}

	private IEnumerator DeathRoutine()
	{
		WeaverAudioPlayer scream = WeaverAudio.Play(Sounds.GrimmScream, transform.position, autoPlay: false);
		scream.AudioSource.PlayDelayed(0.5f);

		WeaverEvents.BroadcastEvent("HEARTBEAT STOP");

		//TODO : Shake Camera AverageShake

		WeaverAudioPlayer swordDeath = WeaverAudio.Play(AudioAssets.EnemyDeathBySword, transform.position);
		swordDeath.AudioSource.pitch = 0.75f;

		WeaverAudioPlayer enemyDamage = WeaverAudio.Play(AudioAssets.DamageEnemy, transform.position);
		enemyDamage.AudioSource.pitch = 0.75f;

		WeaverAudioPlayer endingTune = WeaverAudio.Play(Sounds.EndingTune, transform.position, autoPlay: false);
		endingTune.AudioSource.PlayDelayed(0.3f);

		TransformUtilities.SpawnRandomObjects(EffectAssets.GhostSlash1Prefab, transform.position, 8, 8, 2f, 35f, 0f, 360f);
		TransformUtilities.SpawnRandomObjects(EffectAssets.GhostSlash2Prefab, transform.position, 2, 3, 2f, 35f, 0f, 360f);

		DeathBurst.SetActive(true);

		GrimmAnimator.PlayAnimation("Death Stun");

		FacePlayer(false);

		GrimmRigidbody.velocity = Vector2.zero;

		//TODO Set Audio Snapsho
		AudioMixer gameMixer = WeaverAudio.MainMixer;
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

		Coroutine jitterRoutine = StartCoroutine(TransformUtilities.JitterObject(gameObject, new Vector3(0.2f, 0.2f, 0f)));

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

			ParticleSystem.EmissionModule particleModule = DeathPuff.emission;
			ParticleSystem.MainModule mainModule = DeathPuff.main;

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

		Invisible = true;

		explosions.Play();

		DeathExplosion.Play();

		WeaverAudio.Play(AudioAssets.BossExplosionUninfected, transform.position);

		WeaverEvents.BroadcastEvent("GRIMM DEFEATED");

		yield break;
	}
}
