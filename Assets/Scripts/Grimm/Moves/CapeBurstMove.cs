using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class CapeBurstMove : GrimmMove
{
	[SerializeField]
	Vector2 spawnPosition = new Vector2(85.77f, 13.5f);

	[SerializeField]
	float anticTime = 0.5f;

	[SerializeField]
	Vector3 BurstOffset = new Vector3(0,0,0.1f);

	[SerializeField]
	float burstScale = 1f;

	[SerializeField]
	float startDelay = 0.1f;

	[SerializeField]
	float endDelay = 0.5f;

	[SerializeField]
	Vector2 JitterAmount = new Vector2(0.02f, 0.02f);
	[SerializeField]
	float jitterFPS = 12f;


	[Header("Homing Balls")]
	[SerializeField]
	Vector3 homingBallSpawnOffset = default(Vector3);

	[SerializeField]
	float homingBallVelocity = 5f;
	[SerializeField]
	float homingBallRotationSpeed = 25f;

	[Header("Direct Shots")]
	[SerializeField]
	int amountOfDirectShots = 35;
	[SerializeField]
	float timeBetweenDirectShots = 0.05f;
	[SerializeField]
	Vector2 GlowScale = Vector2.one;
	[SerializeField]
	Vector2 FirePillarOffset = Vector2.zero;

	[Header("Spray Shots")]
	[SerializeField]
	int amountOfWaves = 15;
	[SerializeField]
	int shotsPerWave = 7;
	[SerializeField]
	float angleBetweenSprayShots = 60f;
	[SerializeField]
	float sprayShotVelocity = 20f;
	[SerializeField]
	float sprayShotRotationSpeed = 0f;
	//[SerializeField]
	//Vector2 timeBetweenWavesMinMax = new Vector2(0.05f,0.3f);
	[SerializeField]
	Vector2 timeBetweenWavesMinRange = new Vector2(0.18f, 0.25f);
	[SerializeField]
	Vector2 timeBetweenWavesMaxRange = new Vector2(0.43f, 0.5f);


	[Header("Other")]
	[SerializeField]
	float pitchLow = 1f;
	[SerializeField]
	float pitchHigh = 1.2f;

	float startHealth3rdStage;
	float endHealth3rdStage = 0;
	float healthPercentage = 0f;



	Coroutine jitterRoutine = null;

	void Awake()
	{
		
		//MoveEnabled = true;
	}

	public override IEnumerator DoMove()
	{
		startHealth3rdStage = Grimm.MaxHealth - ((Grimm.MaxHealth / 3) * 2);
		healthPercentage = Mathf.InverseLerp(endHealth3rdStage, startHealth3rdStage, Grimm.GrimmHealth.Health);
		if (!Grimm.Settings.hardMode || Grimm.BossStage == 1)
		{
			yield break;
		}
		transform.position = transform.position.With(x: spawnPosition.x, y: spawnPosition.y);

		while (Vector3.Distance(transform.position, Player.Player1.transform.position) < 6f)
		{
			yield return null;
		}

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn(true,"Cape Teleport In");

		//yield return GrimmAnimator.PlayAnimation("Cape Prepare");
		GrimmAnimator.PlayAnimation("Cape Prepare");

		yield return new WaitForSeconds(anticTime);

		yield return GrimmAnimator.PlayAnimationTillDone("Cape Open");

		jitterRoutine = StartCoroutine(TransformUtilities.JitterObject(gameObject, JitterAmount, jitterFPS));

		yield return GrimmAnimator.PlayAnimationTillDone("Cape Open Finish");

		GrimmAnimator.PlayAnimation("Cape Open Loop");

		yield return new WaitForSeconds(startDelay);

		if (Grimm.BossStage == 2)
		{
			yield return FireDirectShots(amountOfDirectShots, timeBetweenDirectShots);
		}
		else
		{
			yield return FireSprayShots(amountOfWaves,shotsPerWave,angleBetweenSprayShots);
		}

		yield return new WaitForSeconds(endDelay);

		StopCoroutine(jitterRoutine);
		jitterRoutine = null;
		yield return GrimmAnimator.PlayAnimationTillDone("Cape Close");
		yield return Grimm.TeleportOut(true, "Cape Teleport Out");

		if (Grimm.Settings.hardMode && Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.35f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}


		yield break;
	}

	IEnumerator FireDirectShots(int amountOfShots, float timeBetweenShots)
	{
		for (float i = 0; i < amountOfShots; i++)
		{
			var pitch = Mathf.Lerp(pitchLow, pitchHigh, i / (amountOfShots - 1));
			SpawnHomingBall(VectorUtilities.GetAngleBetween(transform.position, Player.Player1.transform.position), homingBallVelocity, homingBallRotationSpeed,true, pitch);
			yield return new WaitForSeconds(timeBetweenShots);
		}
	}

	IEnumerator FireSprayShots(int amountOfWaves, int shotsPerWave, float angleBetweenShots)
	{
		var downAngle = VectorUtilities.VectorToDegrees(Vector2.down);

		bool evenWave = true;
		for (float i = 0; i < amountOfWaves; i++)
		{
			int trueAmountOfShots = shotsPerWave;
			if (evenWave && shotsPerWave % 2 == 1)
			{
				trueAmountOfShots++;
			}

			var angles = VectorUtilities.CalculateSpacedValues(trueAmountOfShots, angleBetweenShots);

			for (int j = 0; j < trueAmountOfShots; j++)
			{
				//var angle = downAngle + Mathf.Lerp(angleBetweenShots, -angleBetweenShots, j / (trueAmountOfShots - 1));
				var angle = downAngle + angles[j];
				//WeaverLog.Log("Angle = " + angle);
				//WeaverLog.Log("Lerp Val = " + (j / (trueAmountOfShots - 1)));
				SpawnHomingBall(angle, sprayShotVelocity, sprayShotRotationSpeed, j == trueAmountOfShots - 1, Mathf.Lerp(pitchLow,pitchHigh,i / (amountOfWaves - 1)));
			}
			evenWave = !evenWave;

			var minTime = Mathf.Lerp(timeBetweenWavesMinRange.x, timeBetweenWavesMinRange.y,healthPercentage);
			var maxTime = Mathf.Lerp(timeBetweenWavesMaxRange.x, timeBetweenWavesMaxRange.y,healthPercentage);

			yield return new WaitForSeconds(Mathf.Lerp(maxTime, minTime, i / (amountOfWaves - 1)));
		}
	}


	public void PlayBurst()
	{
		var instance = GameObject.Instantiate(Grimm.Prefabs.RedBurst, transform.position + BurstOffset, Quaternion.identity);
		instance.transform.localScale = new Vector3(burstScale, burstScale, 1f);
	}

	public void PlayScreamSound()
	{
		WeaverAudio.Play(Sounds.GrimmScream, transform.position);
	}

	public void PlayCapeSound()
	{
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);
	}

	HomingBall SpawnHomingBall(float angle, float velocity, float rotationSpeed, bool playEffects = true, float audioPitch = 1f)
	{
		return SpawnHomingBall(angle, velocity, rotationSpeed, transform.position,playEffects, audioPitch);
	}

	HomingBall SpawnHomingBall(float angle, float velocity, float rotationSpeed, Vector3 position, bool playEffects = true, float audioPitch = 1f)
	{
		HomingBall.GlowScale = GlowScale;
		HomingBall.FirePillarOffset = FirePillarOffset;
		WeaverLog.Log("Spawning Ball");
		var homingBall = HomingBall.Fire(Grimm, position + homingBallSpawnOffset, angle, velocity, rotationSpeed, playEffects, audioPitch);
		homingBall.EnablePhase1 = false;
		homingBall.Phase2Velocity = velocity;
		homingBall.Phase2TravelDirection = VectorUtilities.DegreesToVector(angle, velocity);
		HomingBall.GlowScale = Vector2.one;
		HomingBall.FirePillarOffset = Vector2.zero;
		return homingBall;
	}

	public override void OnStun()
	{

		
	}
}

