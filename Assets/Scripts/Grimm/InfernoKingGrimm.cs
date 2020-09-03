using Assets.Scripts;
using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;
using WeaverCore.Assets;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DamageHero))]
public class InfernoKingGrimm : BossReplacement
{
	private BalloonMove balloonMove;

	bool balloonMoveNext = false;

	public Animator GrimmAnimator { get; private set; }
	public BoxCollider2D GrimmCollider { get; private set; }
	public GrimmSounds Sounds { get; private set; }
	public WeaverAudioPlayer VoicePlayer { get; private set; }
	public EntityHealth GrimmHealth { get; private set; }
	public Rigidbody2D GrimmRigidbody { get; private set; }
	public GrimmDirection FaceDirection { get; private set; }
	public bool Stunned { get; private set; }
	public float MaxHealth { get; set; }
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
	private AudioMixerSnapshot SilentSnapshot;

	[SerializeField]
	string titleLarge = "Grimm";
	[SerializeField]
	string titleSmall = "Inferno King";
	/*[SerializeField]
	AnimationCurve cameraColorCurve;*/
	/*[SerializeField]
	int easyModeHealth = 1300;
	[SerializeField]
	int hardModeHealth = 2100;*/

	[Header("Easy Mode")]
	[SerializeField]
	int easyAttunedHealth = 1300;
	[SerializeField]
	int easyAscendedHealth = 1450;
	[SerializeField]
	int easyRadiantHealth = 1600;

	[Header("Hard Mode")]
	[SerializeField]
	int hardAttunedHealth = 1950;
	[SerializeField]
	int hardAscendedHealth = 2000;
	[SerializeField]
	int hardRadiantHealth = 2050;

	public IKGSettings Settings;
	public GrimmColors Colors;
	public Material CameraMaterial;

	CameraHueShift cameraHueShifter;

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
	private WeaverGameManager.TimeFreezePreset freezePreset;


	/*[Header("Camera Colors A")]
	[SerializeField]
	float shiftPercentageA = 1f;
	[SerializeField]
	float hueShiftA = 0.5f;
	[SerializeField]
	float satShiftA = 0f;
	[SerializeField]
	float valueShiftA = 0f;

	[Header("Camera Colors B")]
	[SerializeField]
	float shiftPercentageB = 1f;
	[SerializeField]
	float hueShiftB = 0.1f;
	[SerializeField]
	float satShiftB = 0.1f;
	[SerializeField]
	float valueShiftB = 0.1f;*/

	[Header("Camera Color Curves")]
	[SerializeField]
	AnimationCurve ShaderPercentageCurve;
	[SerializeField]
	AnimationCurve HueShiftCurve;
	[SerializeField]
	AnimationCurve SaturationShiftCurve;
	[SerializeField]
	AnimationCurve ValueShiftCurve;

	List<GrimmMove> randomMoveStorage;
	int randomMoveIndex = 0;


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

	protected override void Awake()
	{
		base.Awake();
		MainPrefabs.Instance = prefabs;

		var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();


		foreach (var obj in allObjects)
		{
			if (obj.GetComponent<TMP_Text>() != null && obj.scene != null && obj.scene.name != null)
			{
				ChangeTitles(obj);
			}
		}
	}

	private void ChangeTitles(GameObject titleObject)
	{
		if (titleObject != null && CoreInfo.LoadState == RunningState.Game)
		{
			var text = titleObject.GetComponent<TMP_Text>();
			if (text != null)
			{
				var setter = titleObject.GetComponent<TMProTextSetter>();
				if (setter == null)
				{
					setter = titleObject.AddComponent<TMProTextSetter>();
				}
				setter.grimm = this;
			}
		}
	}

	// Use this for initialization
	private void Start()
	{
		//BossStage = 2;
		//GrimmHue.SetAllGrimmHues(0f, 0f, 0f);
		Colors.SetHues(0f, 0f, 0f);
		AddMoves(GetComponents<GrimmMove>());
		balloonMove = GetComponent<BalloonMove>();


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
		GrimmHealth = GetComponent<EntityHealth>();
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

		if (CoreInfo.LoadState == RunningState.Game)
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

		if (Settings.enableCustomHealth)
		{
			GrimmHealth.Health = Settings.CustomHealthValue;
		}
		else
		{
			switch (Diffculty)
			{
				case WeaverCore.DataTypes.BossDifficulty.Attuned:
					GrimmHealth.Health = Settings.hardMode ? hardAttunedHealth : easyAttunedHealth;
					break;
				case WeaverCore.DataTypes.BossDifficulty.Ascended:
					GrimmHealth.Health = Settings.hardMode ? hardAscendedHealth : easyAscendedHealth;
					break;
				case WeaverCore.DataTypes.BossDifficulty.Radiant:
					GrimmHealth.Health = Settings.hardMode ? hardRadiantHealth : easyRadiantHealth;
					break;
			}
		}

		if (Settings.hardMode)
		{
			WeaverLog.Log("IKG : Hard Mode Enabled");
		}
		else
		{
			WeaverLog.Log("IKG : Hard Mode Disabled");
		}

		WeaverLog.Log("IKG : Difficulty = " + Diffculty);
		WeaverLog.Log("IKG : Health = " + GrimmHealth.Health);
		/*GrimmHealth.Health = easyModeHealth;
		if (Settings.hardMode)
		{
			GrimmHealth.Health = hardModeHealth;
		}*/
		MaxHealth = GrimmHealth.Health;

		var quarterHealth = GrimmHealth.Health / 4;
		var thirdHealth = GrimmHealth.Health / 3;

		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - quarterHealth, DoBalloonMove);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 2), DoBalloonMove);
		GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 3), DoBalloonMove);
		AddStunMilestone(GrimmHealth.Health - thirdHealth);
		AddStunMilestone(GrimmHealth.Health - (thirdHealth * 2));

		/*if (Settings.hardMode)
		{
			var fifthHealth = GrimmHealth.Health / 5;
			var quarterHealth = GrimmHealth.Health / 4;

			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - fifthHealth, DoBalloonMove);
			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (fifthHealth * 2), DoBalloonMove);
			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (fifthHealth * 3), DoBalloonMove);
			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (fifthHealth * 4), DoBalloonMove);
			AddStunMilestone(GrimmHealth.Health - quarterHealth);
			AddStunMilestone(GrimmHealth.Health - (quarterHealth * 2));
			AddStunMilestone(GrimmHealth.Health - (quarterHealth * 3));
		}
		else
		{
			var quarterHealth = GrimmHealth.Health / 4;
			var thirdHealth = GrimmHealth.Health / 3;

			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - quarterHealth, DoBalloonMove);
			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 2), DoBalloonMove);
			GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (quarterHealth * 3), DoBalloonMove);
			//GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (thirdHealth), Stun);
			//GrimmHealth.AddHealthMilestone(GrimmHealth.Health - (thirdHealth * 2), Stun);
			AddStunMilestone(GrimmHealth.Health - thirdHealth);
			AddStunMilestone(GrimmHealth.Health - (thirdHealth * 2));
		}*/

		var snapshots = Resources.FindObjectsOfTypeAll<AudioMixerSnapshot>();
		foreach (var snapshot in snapshots)
		{
			if (snapshot.name == "Silent")
			{
				SilentSnapshot = snapshot;
				break;
			}
		}

		if (Settings.hardMode && !Settings.DisableColorEffects)
		{
			if (cameraHueShifter == null)
			{
				cameraHueShifter = WeaverCam.Instance.gameObject.GetComponent<CameraHueShift>();
				if (cameraHueShifter == null)
				{
					cameraHueShifter = WeaverCam.Instance.gameObject.AddComponent<CameraHueShift>();
				}
				cameraHueShifter.cameraMaterial = CameraMaterial;
				cameraHueShifter.ShiftPercentage = 0f;
				//WeaverLog.Log("Shift Percentage = " + cameraHueShifter.ShiftPercentage);
				SceneManager.sceneLoaded += OnSceneChange;
			}
		}
	}

	private void Update()
	{
		if (Settings.hardMode && !Settings.DisableColorEffects)
		{
			var t = 1f - (GrimmHealth.Health / MaxHealth);
			cameraHueShifter.SetValues(ShaderPercentageCurve.Evaluate(t),
				HueShiftCurve.Evaluate(t),
				SaturationShiftCurve.Evaluate(t),
				ValueShiftCurve.Evaluate(t));
			/*var t = cameraColorCurve.Evaluate(1 - (GrimmHealth.Health / maxHealth));
			cameraHueShifter.SetValues(Mathf.Lerp(shiftPercentageA,shiftPercentageB, t),
				Mathf.Lerp(hueShiftA, hueShiftB, t),
				Mathf.Lerp(satShiftA, satShiftB, t),
				Mathf.Lerp(valueShiftA, valueShiftB, t));*/
			//cameraHueShifter.ShiftPercentage = cameraColorCurve.Evaluate(1 - (GrimmHealth.Health / maxHealth));
		}
	}

	private IEnumerator Waiter(float waitTime, Action OnDone)
	{
		yield return new WaitForSeconds(waitTime);
		OnDone();
	}

	//Called when the boss starts
	private void Wake(string eventName, GameObject source)
	{
		if (eventName == "WAKE" && (source == gameObject || source.name.Contains("Grimm Control")))
		{
			WeaverLog.Log("Starting Inferno King Grimm Boss fight");
			transform.position = StartingPosition;

			BossRoutine = CoroutineUtilities.RunCoroutineWhile(this, MainBossControl(), () => !Stunned);
		}
	}

	private IEnumerator MainBossControl()
	{
		yield return new WaitForSeconds(0.6f);
		foreach (var randomMove in RandomMoveIter())
		{
			if (balloonMoveNext)
			{
				balloonMoveNext = false;
				yield return RunMove(balloonMove);
			}
			else
			{
				yield return RunMove(randomMove);
			}
		}
	}

	public IEnumerator TeleportIn(bool playSound = true, string animationName = "Tele In")
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

			yield return GrimmAnimator.PlayAnimationTillDone(animationName);
			GrimmCollider.enabled = true;
		}
	}

	public IEnumerator TeleportOut(bool playSound = true, string animationName = "Tele Out")
	{
		if (!invisible)
		{
			if (playSound)
			{
				WeaverAudio.Play(Sounds.GrimmTeleportOut, transform.position, 1.0f, AudioChannel.Sound);
			}
			GrimmCollider.enabled = false;

			yield return GrimmAnimator.PlayAnimationTillDone(animationName);

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
	}

	private void PlayTeleportParticles()
	{
		teleSmokeBack.Stop();
		teleSmokeBack.Play();

		teleSmokeFront.Stop();
		teleSmokeFront.Play();
	}

	/*public void Stun()
	{
		
	}*/

	protected override void OnStun()
	{
		base.OnStun();
		Stunned = true;

		GrimmCollider.enabled = false;

		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}

		StartCoroutine(StunRoutine());
	}

	public void DoBalloonMove()
	{
		balloonMoveNext = true;
	}

	private IEnumerator StunRoutine()
	{
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

		//yield return new WaitForSeconds(2f);

		float stunWaitTime = 2f;

		//if (!Settings.hardMode)
		//{
		for (float t = 0; t < stunWaitTime; t += Time.deltaTime)
		{
			if (!Settings.hardMode && !Settings.DisableColorEffects)
			{
				if (BossStage == 2)
				{
					//GrimmHue.SetAllGrimmHues(Mathf.Lerp(0f, 0.30f,t / stunWaitTime), 0f, 0f);
					Colors.SetHues(Mathf.Lerp(0f, 0.30f, t / stunWaitTime), 0f, 0f);
				}
				else if (BossStage == 3)
				{
					//GrimmHue.SetAllGrimmHues(Mathf.Lerp(0.30f, 0.45f, t / stunWaitTime), 0f, 0f);
					Colors.SetHues(Mathf.Lerp(0.30f, 0.45f, t / stunWaitTime), 0f, 0f);
				}
			}

			yield return null;
		}

		//}

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

	protected override void OnDeath()
	{
		base.OnDeath();
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
		//Debugger.Log("Boss Dead");
		WeaverLog.Log("Inferno King Grimm Boss Defeated");

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

	/*private struct SnapshotHolder
	{
		public AudioMixerSnapshot snapshot;
	}*/

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

		yield return null;

		GrimmAnimator.PlayAnimation("Death Stun");

		FacePlayer(false);

		GrimmRigidbody.velocity = Vector2.zero;

		//TODO Set Audio Snapsho
		//AudioMixer gameMixer = WeaverAudio.MainMixer;
		/*if (gameMixer != null && ImplFinder.State == RunningState.Game)
		{
			SnapshotHolder snapshot = JsonUtility.FromJson<SnapshotHolder>("{\"snapshot\":{ \"m_FileID\":" + 15948 + ",\"m_PathID\":" + 0 + "}}");

			if (snapshot.snapshot != null)
			{
				snapshot.snapshot.TransitionTo(1f);
			}
		}*/
		if (CoreInfo.LoadState == RunningState.Game)
		{
			SilentSnapshot.TransitionTo(1f);
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

	void OnSceneChange(Scene scene, LoadSceneMode loadMode)
	{
		SceneManager.sceneLoaded -= OnSceneChange;
		cameraHueShifter.ShiftPercentage = 0f;
	}

	//void MaterialDebug()
	//{
		/*if (WeaverCam.Instance.GetComponent<CameraHueShift>() == null)
		{
			var camComponent = WeaverCam.Instance.gameObject.AddComponent<CameraHueShift>();
			camComponent.cameraMaterial = CameraMaterial;
			//camComponent.HueShiftRed = 1f;
		}*/

		/*Dictionary<Material, List<Renderer>> AllMaterials = new Dictionary<Material, List<Renderer>>();

		foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
		{
			var material = renderer.sharedMaterial;
			if (AllMaterials.ContainsKey(material))
			{
				AllMaterials[material].Add(renderer);
			}
			else
			{
				AllMaterials.Add(material, new List<Renderer>() { renderer });
			}
		}

		foreach (var pair in AllMaterials)
		{
			WeaverLog.Log("---Material: " + pair.Key.name + "-----------------------");
			foreach (var renderer in pair.Value)
			{
				WeaverLog.Log(renderer.gameObject.name);
			}
		}*/
	//}

	/*IEnumerator MusicFader(float fadeTime = 0.5f)
	{
		float originalVolume = 
	}*/
}
