using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;
using Random = UnityEngine.Random;

public class BalloonMove : GrimmMove
{
	[SerializeField]
	Vector2 balloonPosition = new Vector2(85.77f,13.5f);

	//[SerializeField]
	//float ballSpawnRate = 0.2f;

	[SerializeField]
	float ballAngleMin = -20f;

	[SerializeField]
	float ballAngleMax = 20f;

	[SerializeField]
	float ballInitialVelocityMin = 2f;

	[SerializeField]
	float ballInitialVelocityMax = 10f;

	[SerializeField]
	float homingAttackTime = 10f;

	[SerializeField]
	float homingBallRotationSpeed = 2f;

	[SerializeField]
	float homingBallVelocity = 2f;

	[SerializeField]
	Vector3 HomingBallOffset;

	[SerializeField]
	float homingAttackTimeIncrement = 0.5f;

	[SerializeField]
	float homingSpawnRateIncrement = 0.05f;

	[Header("Easy Mode")]
	[SerializeField]
	float easySpawnRateStage1 = 0.22f;
	[SerializeField]
	float easySpawnRateStage2 = 0.21f;
	[SerializeField]
	float easySpawnRateStage3 = 0.205f;

	[Header("Intermediate Mode")]
	[SerializeField]
	float mediumSpawnRateStage1 = 0.2f;
	[SerializeField]
	float mediumSpawnRateStage2 = 0.18f;
	[SerializeField]
	float mediumSpawnRateStage3 = 0.17f;

	[Header("Hard Mode")]
	[SerializeField]
	float hardSpawnRateStage1 = 0.2f;
	[SerializeField]
	float hardSpawnRateStage2 = 0.16f;
	[SerializeField]
	float hardSpawnRateStage3 = 0.12f;


	GameObject BalloonFireballShootSound;
	GameObject BalloonCollider;
	EntityHealth healthManager;
	ParticleSystem BalloonParticles;

	//public Vector2 BalloonPosition { get; private set; }
	public bool DoingBalloonMove { get; private set; }
	public int BalloonMoveTimes { get; private set; }

	[Header("God Mode")]
	[Space]
	[SerializeField]
	float godModeSpacing = 5f;

	void Awake()
	{
		healthManager = GetComponent<EntityHealth>();

		BalloonParticles = GetChildObject<ParticleSystem>("Balloon Particles");
		BalloonFireballShootSound = GetChildGameObject("Balloon Fireball Loop Audio");
		BalloonCollider = GetChildGameObject("Balloon Collider");
	}

	public override IEnumerator DoMove()
	{
		if (Grimm.Settings.PufferFishDifficulty == PufferFishDifficulty.Off)
		{
			yield break;
		}

		DoingBalloonMove = true;
		healthManager.Invincible = true;

		//Debug.Log("Doing Balloon Move for grimm " + InfernoKingGrimm.GrimmsFighting.IndexOf(Grimm));

		transform.position = transform.position.With(x: balloonPosition.x, y: balloonPosition.y);
		if (InfernoKingGrimm.GodMode)
		{
			if (InfernoKingGrimm.MainGrimm == Grimm)
			{
				transform.SetXPosition(balloonPosition.x - godModeSpacing);
				foreach (var grimm in InfernoKingGrimm.GrimmsFighting)
				{
					//Debug.Log("Grimm Number = " + InfernoKingGrimm.GrimmsFighting.IndexOf(grimm));
					if (grimm != Grimm)
					{
						//Debug.Log("Starting Balloon Move for it");
						grimm.StartCoroutine(grimm.GetComponent<BalloonMove>().DoMove());
					}
					//else
					//{
						//Debug.Log("Skipping over it");
					//}
				}
			}
			else
			{
				transform.SetXPosition(balloonPosition.x + godModeSpacing);
			}
		}

		while (Vector3.Distance(transform.position, Player.Player1.transform.position) < 6f)
		{
			yield return null;
		}

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		yield return GrimmAnimator.PlayAnimationTillDone("Balloon Antic");

		//TODO - BROADCAST THE CROWD GASP EVENT

		WeaverEvents.BroadcastEvent("CROWD GASP");
		//EventReceiver.BroadcastEvent("CROWD GASP");

		//TODO Broadcast HEART HALFWAY EVENT
		if (Grimm.BossStage == 2)
		{
			WeaverEvents.BroadcastEvent("HEART HALFWAY");
		}

		BalloonFireballShootSound.SetActive(true);

		WeaverAudio.PlayAtPoint(Sounds.GrimmScream, transform.position);
		WeaverAudio.PlayAtPoint(Sounds.InflateSoundEffect, transform.position);

		//TODO Broadcast CROWD CLAP DELAY EVENT

		WeaverEvents.BroadcastEvent("CROWD CLAP DELAY");

		var angle = Random.Range(0f, 360f);

		GrimmAnimator.PlayAnimation("Balloon");

		Grimm.FacePlayer();

		BalloonCollider.SetActive(true);

		CameraShaker.Instance.Shake(ShakeType.AverageShake);
		CameraShaker.Instance.SetRumble(RumbleType.RumblingSmall);

		BalloonParticles.Play();

		var attackTime = homingAttackTime + (homingAttackTimeIncrement * (Grimm.BossStage - 1));
		//var spawnRate = ballSpawnRate - (homingSpawnRateIncrement * (Grimm.BossStage - 1));
		float spawnRate = 0f;

		var difficulty = Grimm.Settings.PufferFishDifficulty;

		if (InfernoKingGrimm.GodMode)
		{
			if (Grimm.Settings.hardMode)
			{
				difficulty = PufferFishDifficulty.Intermediate;
			}
			else
			{
				difficulty = PufferFishDifficulty.Easy;
			}
		}

		switch (difficulty)
		{
			case PufferFishDifficulty.Easy:
				switch (Grimm.BossStage)
				{
					case 1:
						spawnRate = easySpawnRateStage1;
						break;
					case 2:
						spawnRate = easySpawnRateStage2;
						break;
					default:
						spawnRate = easySpawnRateStage3;
						break;
				}
				break;
			case PufferFishDifficulty.Intermediate:
				switch (Grimm.BossStage)
				{
					case 1:
						spawnRate = mediumSpawnRateStage1;
						break;
					case 2:
						spawnRate = mediumSpawnRateStage2;
						break;
					default:
						spawnRate = mediumSpawnRateStage3;
						break;
				}
				break;
			case PufferFishDifficulty.Hard:
				switch (Grimm.BossStage)
				{
					case 1:
						spawnRate = hardSpawnRateStage1;
						break;
					case 2:
						spawnRate = hardSpawnRateStage2;
						break;
					default:
						spawnRate = hardSpawnRateStage3;
						break;
				}
				break;
			default:
				switch (Grimm.BossStage)
				{
					case 1:
						spawnRate = Grimm.Settings.hardMode ? hardSpawnRateStage1 : mediumSpawnRateStage1;
						break;
					case 2:
						spawnRate = Grimm.Settings.hardMode ? hardSpawnRateStage2 : mediumSpawnRateStage2;
						break;
					default:
						spawnRate = Grimm.Settings.hardMode ? hardSpawnRateStage3 : mediumSpawnRateStage3;
						break;
				}
				break;
		}

		float rateCounter = 0f;

		//HashSet<HomingBall> homingBalls = new HashSet<HomingBall>();

		for (float t = 0; t < attackTime; t += Time.deltaTime)
		{
			rateCounter += Time.deltaTime;
			if (rateCounter >= spawnRate)
			{
				rateCounter -= spawnRate;

				var spawnAngle = Random.Range(ballAngleMin, ballAngleMax);
				var spawnVelocity = Random.Range(ballInitialVelocityMin, ballInitialVelocityMax);

				if (Random.value > 0.5f)
				{
					spawnAngle = -spawnAngle;
					spawnVelocity = -spawnVelocity;
				}

				var homingBall = HomingBall.Fire(Grimm, transform.position + HomingBallOffset, spawnAngle, spawnVelocity, homingBallRotationSpeed, false);
				homingBall.Phase2Velocity = homingBallVelocity;
				if (Grimm.BossStage == 1)
				{
					homingBall.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
				}
				else if (Grimm.BossStage == 2)
				{
					homingBall.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
				}
				else
				{
					homingBall.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
				}
				homingBall.Phase2TargetOffset = new Vector2(Random.Range(-4f,4f),Random.Range(-2f,2f));
				//homingBalls.Add(homingBall);
			}

			yield return null;

		}

		/*for (int i = 0; i < amountOfWaves; i++)
		{
			yield return new WaitForSeconds(0.6f);
			ballCounter++;
			waveCounter++;

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? -7.4f : -5.4f, -10f, -4.5f);

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 0f : -3f, -10f, -4.5f);

			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 3f : 5f, 10f, 4.5f);
			GrimmBall.Spawn(ballSpawn, Random.value > 0.5f ? 3f : 5f, -10f, -4.5f);
		}*/

		CameraShaker.Instance.SetRumble(RumbleType.None);

		yield return new WaitForSeconds(1.75f);

		BalloonParticles.Stop();

		BalloonCollider.SetActive(false);

		//TODO - Broadcast CROWD IDLE EVENT

		WeaverEvents.BroadcastEvent("CROWD IDLE");

		BalloonFireballShootSound.SetActive(false);

		WeaverAudio.PlayAtPoint(Sounds.DeflateSoundEffect, transform.position);

		yield return Grimm.TeleportOut();

		healthManager.Invincible = false;

		yield return new WaitForSeconds(0.3f);

		foreach (var homingBall in HomingBall.ActiveHomingBalls)
		{
			if (homingBall != null)
			{
				homingBall.ShrinkAndStop();
			}
		}

		if (Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			yield return new WaitForSeconds(0.3f);
		}

		DoingBalloonMove = false;
		BalloonMoveTimes++;
	}

	public override void OnStun()
	{
		DoingBalloonMove = false;
		CameraShaker.Instance.SetRumble(RumbleType.None);
		BalloonFireballShootSound.SetActive(false);
		BalloonParticles.Stop();
	}
}

