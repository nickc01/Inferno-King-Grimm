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
using Modding;
using WeaverCore.Implementations;
using System.IO;
using Newtonsoft.Json;
using IKG;

[RequireComponent(typeof(DamageHero))]
public class InfernoKingGrimm : BossReplacement
{
	public static InfernoKingGrimm MainGrimm
	{
		get
		{
			if (GrimmsFighting.Count > 0)
			{
				return GrimmsFighting[0];
			}
			else
			{
				return null;
			}
		}
	}
	public static List<InfernoKingGrimm> GrimmsFighting = new List<InfernoKingGrimm>();

	/// <summary>
	/// A speed value determining how fast the boss should be. In Infinite mode, this value increases gradually
	/// </summary>
	public static float InfiniteSpeed = 1.0f;

	public static float MultipliedInfiniteSpeed(float multiplier)
	{
		return ((InfiniteSpeed - 1f) * multiplier) + 1f;
	}

	/// <summary>
	/// How much health the boss needs to have before the speed doubles. In the Awake function, this is set to 2 Boss Cycles. Every 2 Boss Cycles (6 phases), the speed will double
	/// </summary>
	public static float DoublingRatio;

	private BalloonMove balloonMove;

	//IEnumerable<GrimmHooks> Hooks = ReflectionUtilities.GetObjectsOfType<GrimmHooks>();

	bool balloonMoveNext = false;

	//public static InfernoKingGrimm Instance { get; private set; }
	public int CycleAmount { get; private set; }
	public int CurrentCycle { get; private set; }
	public Animator GrimmAnimator { get; private set; }
	public BoxCollider2D GrimmCollider { get; private set; }
	public GrimmSounds Sounds { get; private set; }
	public AudioPlayer VoicePlayer { get; private set; }
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

	//private EventManager receiver;
	private EventListener listener;
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
	/*#if !UNITY_EDITOR
		private AudioMixerSnapshot SilentSnapshot;
	#endif*/

	[SerializeField]
	Material performanceModeMaterial;
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
	//public GrimmColors Colors;
	public Material CameraMaterial;

	static CameraHueShift cameraHueShifter;
	static bool sceneChangeHookUsed = false;
	static bool languageHooksAdded = false;

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
	//public ObjectPoolOLD2<HomingBall> HomingBallPool;

	//public Recycler Recycler { get; private set; }
	//public ObjectPool<HomingBall> HomingBallPool;
	[Space]
	[Header("God Mode")]
	[SerializeField]
	bool forceGodGrimmMode = false;
	[SerializeField]
	bool forceBalloonMove = false;

	bool godModeDoSpikes = true;


	static bool godModeFlag = false;
	public static bool GodMode
	{
		get
		{
#if UNITY_EDITOR
			return godModeFlag;
#else
			return godModeFlag && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Grimm_Nightmare";
#endif
		}
	}

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

	static Mod FindInfiniteGrimmMod()
	{
		foreach (var mod in WeaverMod.LoadedMods)
		{
			var modType = mod.GetType();
			if (modType.Name == "InfiniteGrimm")
			{
				return (Mod)mod;
			}
		}
		return null;
	}

	bool GetGodGrimmFlag()
	{
		if (forceGodGrimmMode)
		{
			return true;
		}
		var mod = FindInfiniteGrimmMod();
		if (mod != null)
		{
			var modType = mod.GetType();

			var globalsType = modType.Assembly.GetType("infinitegrimm.infinite_globals");

			var godModeF = globalsType.GetField("godMode");

			return (bool)godModeF.GetValue(null);

			/*var modType = mod.GetType();
			var globalSettings = modType.GetProperty("GlobalSettings").GetValue(mod, null);

			var gsType = globalSettings.GetType();

			var godMode = gsType.GetProperty("NightmareGodGrimm").GetValue(globalSettings, null);

			return (bool)godMode;*/
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		MainPrefabs.Instance = prefabs;
		if (!languageHooksAdded)
		{
			languageHooksAdded = true;
			Modding.ModHooks.LanguageGetHook += InfernoGrimmMod.TentBossTitleHook;
		}

		/*var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();


		foreach (var obj in allObjects)
		{
			if (obj.GetComponent<TMP_Text>() != null && obj.scene != null && obj.scene.name != null)
			{
				ChangeTitles(obj);
			}
		}*/
	}

/*	private void ChangeTitles(GameObject titleObject)
	{
#if !UNITY_EDITOR
		if (titleObject != null)
		{
			var text = titleObject.GetComponent<TMP_Text>();
			if (text != null)
			{
				var setter = titleObject.GetComponent<TMProTextSetter>();
				if (setter == null)
				{
					setter = titleObject.AddComponent<TMProTextSetter>();
				}
			}
		}
#endif
	}*/

	///Grabbed from the Infinite Grimm Mod
	static readonly string[] LAG_OBJECTS_TO_KILL =
		{
			"Grimm_nightmare__0017_9 (2)", "grimm_curtain (20)", "default_particles",
			"grimm_pole_standard (5)", "water_fog", "Grimm_nightmare__0016_10", "Grimm Control.Grimm_heart",
			"Grimm Control.Crowd", "Grimm Control.Heartbeat Audio", "Grimm_Main_tent_0009_3 (14)", "grimm_rag_main_tent",
			"grimm_rag_main_tent (25)", "grimm_fader", "grimm_fader", "grimm_rag_main_tent (58)", "Grimm_nightmare__0014_12",
			"grimm_rag_main_tent (29)", "Grimm_Main_tent_0007_5 (20)", "grimm_fader (1)", "grimm_rag_main_tent (23)",
			"Grimm_Main_tent_0008_4 (8)", "grimm_wallpaper (12)", "Grimm_Main_tent_0006_6 (7)",
			"grimm_pole_standard (8)", "grimm_rag_main_tent (59)", "Grimm_Main_tent_0010_2 (16)",
			"grimm_rag_main_tent (11)", "grimm_rag_main_tent (44)", "Grimm_nightmare__0020_6", "Grimm_nightmare__0018_8",
			"grimm_curtain (2)", "Grimm_nightmare__0018_8 (5)", "Grimm_Main_tent_0006_6 (11)", "grimm_rag_main_tent (19)",
			"Grimm_nightmare__0016_10 (8)", "Grimm_nightmare__0016_10 (13)", "break_rag (6)", "grimm_fader (12)",
			"Grimm_nightmare__0017_9 (1)", "Grimm_nightmare_fabric_lantern (11)", "Grimm Control.Halfway Glow",
			"Grimm Control.Final Glow", "Grimm Control.Crowd Fader",
			"main_tent_short_pole (8)", "Grimm_nightmare__0014_12 (14)", "Grimm_nightmare__0022_4 (1)",
			"Grimm_nightmare__0018_8 (1)", "Grimm_Main_tent_0006_6 (13)", "Spotlight Appear", "grimm_rag_main_tent (54)",
			"grimm_rag_main_tent (17)", "Grimm_Main_tent_0008_4 (10)", "grimm_pole_bit", "grimm_rag_main_tent (12)",
			"Grimm_nightmare_fabric_lantern (3)", "grimm_rag_main_tent (18)", "Grimm_nightmare__0014_12 (13)",
			"Grimm_Main_tent_0009_3 (10)", "Grimm_nightmare__0014_12 (7)", "Grimm_Main_tent_0008_4 (14)",
			"grimm_rag_main_tent (22)", "Grimm_nightmare__0023_3", "break_rag (5)", "grimm_rag_main_tent (39)",
			"Grimm_nightmare__0019_7", "grimm_wallpaper (5)", "grimm_rag_main_tent (27)", "Grimm_Main_tent_0010_2 (6)",
			"grimm_fader (1)", "Grimm_nightmare__0016_10 (7)", "Grimm_nightmare_fabric_lantern (6)",
			"grimm_rag_main_tent (61)", "Grimm_nightmare__0016_10 (24)", "Grimm_nightmare__0017_9 (10)",
			"grimm_rag_main_tent (45)", "Grimm_nightmare_fabric_lantern (13)", "Grimm_nightmare__0016_10 (21)",
			"grimm_wallpaper (6)", "grimm_curtain (19)", "grimm_rag_main_tent (47)", "grimm_rag_main_tent (2)",
			"grimm_curtain_02 (15)", "Grimm_Main_tent_0006_6 (14)", "dream_particles", "grimm_pole_standard (3)",
			"Grimm_nightmare_fabric_lantern (1)", "break_rag (4)", "Incense Particle", "Grimm_nightmare_fabric_lantern (7)",
			"main_tent_short_pole (5)", "Grimm_nightmare_fabric_lantern (8)", "break_rag (1)", "Grimm_nightmare_fabric_lantern (9)",
			"Grimm_nightmare_fabric_lantern (2)", "Grimm_nightmare_fabric_lantern (5)", "grimm_pole_standard (1)",
			"Grimm_nightmare_fabric_lantern (4)", "Grimm_nightmare_fabric_lantern", "Grimm_nightmare_fabric_lantern (12)",
			"break_rag", "break_rag (3)", "break_rag (2)", "Grimm_nightmare_fabric_lantern (10)", "break_rag (7)"
		};

	void RemoveExtras()
	{
		if (Settings.PerformanceMode)
		{
			var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			if (activeScene.name == "GG_Grimm_Nightmare")
			{
				var decor = GameObject.Find("GG_Arena_Prefab");
				if (decor != null)
				{
					GameObject.Destroy(decor);
				}
				var godseeker = GameObject.Find("Godseeker Crowd");
				if (godseeker != null)
				{
					GameObject.Destroy(godseeker);
				}

				var chunk1 = GameObject.Find("Chunk 0 2");
				if (chunk1 != null)
				{
					chunk1.GetComponent<MeshRenderer>().material = performanceModeMaterial;
				}

				var chunk2 = GameObject.Find("Chunk 0 3");
				if (chunk2 != null)
				{
					chunk2.GetComponent<MeshRenderer>().material = performanceModeMaterial;
				}
			}
			if (activeScene.name == "GG_Grimm_Nightmare" || activeScene.name == "Grimm_Nightmare")
			{
				UnboundCoroutine.Start(Routine());

				IEnumerator Routine()
				{
					yield return new WaitForSeconds(0.1f);
					foreach (string s in LAG_OBJECTS_TO_KILL)
					{
						string[] gameObjs = s.Split('.');
						GameObject killMe = GameObject.Find(gameObjs[0]);

						if (killMe == null)
						{
							continue;
						}

						for (int i = 1; i < gameObjs.Length; i++)
						{
							//killMe = killMe.FindGameObjectInChildren(gameObjs[i]);
							killMe = killMe.transform.Find(gameObjs[i])?.gameObject;
							if (killMe != null) continue;
						}

						if (killMe != null)
							GameObject.Destroy(killMe);
					}
				}
			}
		}
	}

	// Use this for initialization
	private void Start()
	{
		RemoveExtras();
		//Recycler = Recycler.CreateRecycler();
		//BossStage = 2;
		//GrimmHue.SetAllGrimmHues(0f, 0f, 0f);
		//Colors.SetHues(0f, 0f, 0f);
		//AddMoves(GetComponents<GrimmMove>());
		balloonMove = GetComponent<BalloonMove>();


		if ((listener = GetComponent<EventListener>()) == null)
		{
			listener = gameObject.AddComponent<EventListener>();
		}

		GrimmRigidbody = GetComponent<Rigidbody2D>();
		VoicePlayer = GetComponent<AudioPlayer>();
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

		godModeFlag = GetGodGrimmFlag();

		//HomingBallPool = Recycler.CreatePool(Prefabs.HomingBallPrefab, 20, true);
		if (!GodMode && GrimmsFighting.Count > 0)
		{
			StartBossFight();
		}
		else
		{
#if UNITY_EDITOR
			StartCoroutine(Waiter(1, () => Wake(gameObject,null)));
#else
			//receiver.ReceiveAllEventsFromName("WAKE");
			//receiver.AddReceiverForEvent("WAKE", Wake);
			listener.ListenForEvent("WAKE", Wake);
			//receiver.OnReceiveEvent += Wake;
#endif
		}
		GrimmsFighting.Add(this);

		GrimmAnimator.enabled = false;
		spriteRenderer.enabled = false;
		GrimmCollider.enabled = false;

		if (Settings.EnableCustomHealth && !Settings.Infinite)
		{
			GrimmHealth.Health = Settings.CustomHealthValue;
		}
		else
		{
			if (Settings.Infinite)
			{
				GrimmHealth.Health = Settings.hardMode ? hardRadiantHealth : easyRadiantHealth;
			}
			else
			{
				switch (Difficulty)
				{
					case BossDifficulty.Attuned:
						GrimmHealth.Health = Settings.hardMode ? hardAttunedHealth : easyAttunedHealth;
						break;
					case BossDifficulty.Ascended:
						GrimmHealth.Health = Settings.hardMode ? hardAscendedHealth : easyAscendedHealth;
						break;
					case BossDifficulty.Radiant:
						GrimmHealth.Health = Settings.hardMode ? hardRadiantHealth : easyRadiantHealth;
						break;
				}
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

		if (GodMode)
		{
			WeaverLog.Log("IKG : God Mode Enabled. Good luck");
			if (Settings.IncreasedGodModeHealth || Settings.Infinite)
			{
				GrimmHealth.Health = Mathf.RoundToInt(GrimmHealth.Health * 1.75f);
			}
		}

		CycleAmount = GrimmHealth.Health;
		InfiniteSpeed = 1f;
		DoublingRatio = CycleAmount * 5f;

		if (Settings.Infinite)
		{
			WeaverLog.Log("IKG : Infinite Mode Enabled");
			GrimmHealth.HealthDirection = HealthDirection.Up;
			GrimmHealth.Health = 1;
		}
		else
		{
			WeaverLog.Log("IKG : Health = " + GrimmHealth.Health);
		}

		WeaverLog.Log("IKG : Difficulty = " + Difficulty);
		/*GrimmHealth.Health = easyModeHealth;
		if (Settings.hardMode)
		{
			GrimmHealth.Health = hardModeHealth;
		}*/
		MaxHealth = CycleAmount;
		CurrentCycle = -1;


		var quarterHealth = CycleAmount / 4;
		var thirdHealth = CycleAmount / 3;


		if (!GodMode || (GodMode && MainGrimm == this))
		{
			if (!Settings.Infinite)
			{
				GrimmHealth.AddHealthMilestone(CycleAmount - quarterHealth, DoBalloonMove);
				GrimmHealth.AddHealthMilestone(CycleAmount - (quarterHealth * 2), DoBalloonMove);
				GrimmHealth.AddHealthMilestone(CycleAmount - (quarterHealth * 3), DoBalloonMove);
				AddStunMilestone(CycleAmount - thirdHealth);
				AddStunMilestone(CycleAmount - (thirdHealth * 2));
			}
			else
			{
				Action cycleMilestone = null;
				cycleMilestone = () =>
				{
					CurrentCycle++;
					GrimmHealth.AddHealthMilestone((CurrentCycle * CycleAmount) + CycleAmount - quarterHealth, DoBalloonMove);
					GrimmHealth.AddHealthMilestone((CurrentCycle * CycleAmount) + CycleAmount - (quarterHealth * 2), DoBalloonMove);
					GrimmHealth.AddHealthMilestone((CurrentCycle * CycleAmount) + CycleAmount - (quarterHealth * 3), DoBalloonMove);
					AddStunMilestone((CurrentCycle * CycleAmount) + CycleAmount - thirdHealth);
					AddStunMilestone((CurrentCycle * CycleAmount) + CycleAmount - (thirdHealth * 2));
					AddStunMilestone((CurrentCycle * CycleAmount) + CycleAmount);
					GrimmHealth.AddHealthMilestone((CurrentCycle * CycleAmount) + CycleAmount, cycleMilestone);
				};
				cycleMilestone();
			}
		}

		if (GodMode && MainGrimm != this && !Settings.Infinite)
		{
			GrimmHealth.AddHealthMilestone(quarterHealth / 2, () => godModeDoSpikes = false);
		}

		if (GodMode || Settings.Infinite)
		{
			GeoCounter.Instance.GeoText = "0";
		}

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

		/*#if !UNITY_EDITOR
				var snapshots = Resources.FindObjectsOfTypeAll<AudioMixerSnapshot>();
				foreach (var snapshot in snapshots)
				{
					if (snapshot.name == "Silent")
					{
						SilentSnapshot = snapshot;
						break;
					}
				}
		#endif*/
		if ((!Settings.DisableColorEffects && !Settings.PerformanceMode) && !Settings.BlueMode)
		{
			if (cameraHueShifter == null)
			{
				cameraHueShifter = WeaverCamera.Instance.gameObject.GetComponent<CameraHueShift>();
				if (cameraHueShifter == null)
				{
					cameraHueShifter = WeaverCamera.Instance.gameObject.AddComponent<CameraHueShift>();
				}
				cameraHueShifter.cameraMaterial = CameraMaterial;
				cameraHueShifter.ShiftPercentage = 0f;
				cameraHueShifter.Refresh();
				//WeaverLog.Log("Shift Percentage = " + cameraHueShifter.ShiftPercentage);
			}
		}
		if (!sceneChangeHookUsed)
		{
			sceneChangeHookUsed = true;
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneChange;
		}

		ReflectionUtilities.ExecuteMethodsWithAttribute<OnIKGAwakeAttribute>();

		/*if (GodMode)
		{
			EntityHealth.OnHealthChangeEvent += GodGrimmHealthChange;
		}*/

		/*foreach (var hook in Hooks)
		{
			hook.OnGrimmAwake(this);
		}*/
	}

	int healthCache = -1;

	private void Update()
	{
		if ((!Settings.DisableColorEffects && !Settings.PerformanceMode) && !Settings.BlueMode && cameraHueShifter != null)
		{
			if (GrimmHealth.Health != healthCache)
			{
				healthCache = GrimmHealth.Health;
				var t = Mathf.Clamp01(1f - (GrimmHealth.Health / MaxHealth));
				if (Settings.Infinite)
				{
					t = 1 - t;
				}
				if (!Settings.hardMode)
				{
					t *= 0.13f;
				}
				//UnityEngine.Debug.Log("Health Value = " + t);
				var shiftPercentage = ShaderPercentageCurve.Evaluate(t);
				//if (shiftPercentage != cameraHueShifter.ShiftPercentage)
				//{
					cameraHueShifter.SetValues(shiftPercentage,
					HueShiftCurve.Evaluate(t),
					SaturationShiftCurve.Evaluate(t),
					ValueShiftCurve.Evaluate(t));
				//}
				

				/*if (BossStage == 2)
				{
					//GrimmHue.SetAllGrimmHues(Mathf.Lerp(0f, 0.30f,t / stunWaitTime), 0f, 0f);
					//Colors.SetHues(Mathf.Lerp(0f, 0.30f, t / stunWaitTime), 0f, 0f);
				}
				else if (BossStage == 3)
				{
					//GrimmHue.SetAllGrimmHues(Mathf.Lerp(0.30f, 0.45f, t / stunWaitTime), 0f, 0f);
					//Colors.SetHues(Mathf.Lerp(0.30f, 0.45f, t / stunWaitTime), 0f, 0f);
				}*/
			}
		}
	}

	private IEnumerator Waiter(float waitTime, Action OnDone)
	{
		yield return new WaitForSeconds(waitTime);
		OnDone();
	}

	//Called when the boss starts
	private void Wake(GameObject source, GameObject destination)
	{
		/*if ((BossRoutine == null && eventName == "WAKE" && (source == gameObject || source.name.Contains("Grimm Control"))))
		{
			StartBossFight();
		}*/

		if ((BossRoutine == null && (source == gameObject || source.name.Contains("Grimm Control"))))
		{
			StartBossFight();
		}
	}

	void StartBossFight()
	{
		if (BossRoutine == null)
		{
			WeaverLog.Log("IKG : Starting Boss fight");
			transform.position = StartingPosition;

			/*foreach (var hook in Hooks)
			{
				hook.OnGrimmBattleBegin(this);
			}*/

			BossRoutine = CoroutineUtilities.RunCoroutineWhile(this, MainBossControl(), () => !Stunned);
		}
	}

	private IEnumerator MainBossControl()
	{
		yield return new WaitForSeconds(0.6f);
		if (forceBalloonMove)
		{
			DoBalloonMove();
		}
		if (!GodMode)
		{
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
		else
		{
			//if (MainGrimm == this || (MainGrimm != this && !godModeDoSpikes))
			//{
			var spikeMove = GetComponent<SpikesMove>();
			var balloonMove = GetComponent<BalloonMove>();
			foreach (var randomMove in RandomMoveIter())
			{
				if (MainGrimm == this || (MainGrimm != this && !godModeDoSpikes && balloonMove.BalloonMoveTimes >= 3))
				{
					if (balloonMoveNext)
					{
						balloonMoveNext = false;
						yield return RunMove(balloonMove);
					}
					else
					{
						if (randomMove is PillarsMove || randomMove is CapeBurstMove || randomMove is SpikesMove)
						{
							continue;
						}
						else
						{
							yield return RunMove(randomMove);
						}
					}
				}
				else
				{
					if (balloonMove.DoingBalloonMove)
					{
						yield return new WaitUntil(() => !balloonMove.DoingBalloonMove);
						yield return new WaitForSeconds(0.6f / InfiniteSpeed);
					}
					else
					{
						yield return RunMove(spikeMove);
					}
				}
			}
			//}
			//else
			//{

			//}
		}
	}

	IEnumerable<GrimmMove> RandomMoveIter()
	{
		var moves = GetComponents<GrimmMove>().ToList();

		GrimmMove previousMove = null;

		while (true)
		{
			moves.RandomizeList();
			if (previousMove != null && moves.Count > 0 && moves[0] == previousMove)
			{
				moves.Remove(previousMove);
				moves.Add(previousMove);
			}


			bool returnedOnce = false;
			foreach (var move in moves)
			{
				if (move.MoveEnabled)
				{
					returnedOnce = true;
					previousMove = move;
					yield return move;
				}
			}
			if (!returnedOnce)
			{
				yield return null;
			}
		}
	}

	public IEnumerator TeleportIn(bool playSound = true, string animationName = "Tele In")
	{
		if (Invisible)
		{
			if (playSound)
			{
				WeaverAudio.PlayAtPoint(Sounds.GrimmTeleportIn, transform.position, 1.0f, AudioChannel.Sound);
			}

			PlayTeleportParticles();

			Invisible = false;

			//EventManager.BroadcastEvent("EnemyKillShake", gameObject);
			CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

			GrimmAnimator.speed = InfiniteSpeed;
			yield return GrimmAnimator.PlayAnimationTillDone(animationName);
			GrimmAnimator.speed = 1f;
			GrimmCollider.enabled = true;
			Invisible = false;
		}
	}

	public IEnumerator TeleportOut(bool playSound = true, string animationName = "Tele Out")
	{
		if (!invisible)
		{
			if (playSound)
			{
				WeaverAudio.PlayAtPoint(Sounds.GrimmTeleportOut, transform.position, 1.0f, AudioChannel.Sound);
			}
			GrimmCollider.enabled = false;

			Invisible = false;

			GrimmAnimator.speed = InfiniteSpeed;
			yield return GrimmAnimator.PlayAnimationTillDone(animationName);
			GrimmAnimator.speed = 1f;

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
		GrimmAnimator.speed = 1f;
		if (Settings.Infinite && BossStage % 4 == 0)
		{
			BossStage = 1;
		}
		if (!Stunned)
		{
			Stunned = true;
		}
		else
		{
			return;
		}

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

		EventManager.BroadcastEvent("EnemyKillShake", gameObject);

		WeaverGameManager.FreezeGameTime(freezePreset);

		//TODO - Cause the game to freeze temporarily

		yield return null;

		Instantiate(Prefabs.StunEffect, transform.position, Quaternion.identity);

		GrimmRigidbody.velocity = Vector2.zero;

		yield return GrimmAnimator.PlayAnimationTillDone("Explode Antic");

		VoicePlayer.Play(Sounds.GrimmScream);

		//TODO Broadcast Event - CROWD STILL
		WeaverAudio.PlayAtPoint(Sounds.GrimmBatExplosion, transform.position);

		explosions.Play();

		BatController.SendOut(this);

		//TODO - Broadcast Event - CROWD STILL
		EventManager.BroadcastEvent("CROWD STILL", gameObject);

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
			/*if (!Settings.hardMode && !Settings.DisableColorEffects)
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
			}*/

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

		WeaverAudio.PlayAtPoint(Sounds.GrimmTeleportOut, transform.position);
		WeaverAudio.PlayAtPoint(Sounds.GrimmBatsReform, transform.position);

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
		EventManager.BroadcastEvent("CROWD CLAP", gameObject);

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
		GrimmAnimator.speed = 1f;
		base.OnDeath();
		StopAllCoroutines();
		Stunned = true;
		/*if (Stunned == true)
		{
			return;
		}
		Stunned = true;*/

		GrimmCollider.enabled = false;
		if (BossRoutine != null)
		{
			StopCoroutine(BossRoutine);
			BossRoutine = null;
		}
		//Debugger.Log("Boss Dead");
		WeaverLog.Log("IKG : Boss Defeated");

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

		/*foreach (var hook in Hooks)
		{
			hook.OnGrimmBattleEnd(this);
		}*/

		StartCoroutine(DeathRoutine());
	}

	/*private struct SnapshotHolder
	{
		public AudioMixerSnapshot snapshot;
	}*/

	void OnDestroy()
	{
		GrimmsFighting.Remove(this);
		if (Settings.Infinite)
		{
			if (GameObject.FindObjectOfType<InfiniteGrimmDisplay>() != null)
			{
				return;
			}
			var display = GameObject.Instantiate(Prefabs.GrimmDisplayPrefab, WeaverCanvas.Content);

			string bossName = "Infinite ";

			if (Settings.hardMode)
			{
				bossName += "Absolute Inferno ";
			}
			else
			{
				bossName += "Inferno ";
			}

			if (GodMode)
			{
				bossName += "God ";
			}
			else
			{
				bossName += "King ";
			}
			bossName += "Grimm";

			string title = bossName;

			var score = GrimmHealth.Health - 1;

			title += Environment.NewLine + Environment.NewLine;

			title += "Score: " + score;

			var ikgLocation = new FileInfo(typeof(InfernoKingGrimm).Assembly.Location).Directory;

			InfiniteGrimmRecords records = null;

			var recordsPath = PathUtilities.AddSlash(ikgLocation.FullName) + "Infinite Grimm Records.json";

			if (File.Exists(recordsPath))
			{
				try
				{
					records = JsonConvert.DeserializeObject<InfiniteGrimmRecords>(File.ReadAllText(recordsPath));
					if (records == null)
					{
						records = new InfiniteGrimmRecords();
					}
				}
				catch (Exception e)
				{
					records = new InfiniteGrimmRecords();
				}
			}
			else
			{
				records = new InfiniteGrimmRecords();
			}

			if (!records.Records.Any(r => r.Score >= score && r.BossName == bossName))
			{
				title += Environment.NewLine + Environment.NewLine;

				title += "New Record!";
			}

			display.Stats = title;

			records.Records.Add(new Record
			{
				BossName = bossName,
				Date = DateTime.Now.ToString(),
				Score = score
			});

			File.WriteAllText(recordsPath, JsonConvert.SerializeObject(records,Formatting.Indented));
		}
	}

	void OnDisable()
	{
		GrimmsFighting.Remove(this);
	}

	private IEnumerator DeathRoutine()
	{
		GrimmsFighting.Remove(this);
		AudioPlayer scream = WeaverAudio.Create(Sounds.GrimmScream, transform.position);
		scream.PlayDelayed(0.5f);
		//scream.AudioSource.PlayDelayed(0.5f);

		EventManager.BroadcastEvent("HEARTBEAT STOP", gameObject);

		//TODO : Shake Camera AverageShake

		AudioPlayer swordDeath = WeaverAudio.PlayAtPoint(AudioAssets.EnemyDeathBySword, transform.position);
		swordDeath.AudioSource.pitch = 0.75f;

		AudioPlayer enemyDamage = WeaverAudio.PlayAtPoint(AudioAssets.DamageEnemy, transform.position);
		enemyDamage.AudioSource.pitch = 0.75f;

		AudioPlayer endingTune = WeaverAudio.Create(Sounds.EndingTune, transform.position);
		endingTune.PlayDelayed(0.3f);

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
		/*#if !UNITY_EDITOR
				SilentSnapshot.TransitionTo(1f);
		#endif*/

		Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 1f);

		yield return new WaitForSeconds(1f);

		//TODO - HIDE HUD

		Coroutine jitterRoutine = StartCoroutine(TransformUtilities.JitterObject(gameObject, new Vector3(0.2f, 0.2f, 0f)));

		EventManager.BroadcastEvent("HEARTBEAT FAST", gameObject);

		WeaverAudio.PlayAtPoint(AudioAssets.BossFinalHit, transform.position);
		WeaverAudio.PlayAtPoint(AudioAssets.BossGushing, transform.position);

		EventManager.BroadcastEvent("BigShake", gameObject); //???

		DeathPuff.Play();
		SteamParticles.Play();

		//TODO - SHake Camera - BigShake

		float emitRate = 50f;
		float emitSpeed = 5f;

		EventManager.BroadcastEvent("CROWD GASP", gameObject);

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

		WeaverAudio.PlayAtPoint(Sounds.UpperCutExplodeEffect, transform.position);

		EventManager.BroadcastEvent("CROWD STILL", gameObject);

		EventManager.BroadcastEvent("BigShake", gameObject);

		Invisible = true;

		explosions.Play();

		DeathExplosion.Play();

		WeaverAudio.PlayAtPoint(AudioAssets.BossExplosionUninfected, transform.position);

		EventManager.BroadcastEvent("GRIMM DEFEATED", gameObject);

		yield break;
	}

	void OnSceneChange(Scene scene, LoadSceneMode loadMode)
	{
		if (!Settings.BlueMode && cameraHueShifter != null)
		{
			cameraHueShifter.ShiftPercentage = 0f;
			cameraHueShifter.Refresh();
		}
	}
}
