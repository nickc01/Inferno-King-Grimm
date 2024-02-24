using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class CapeBurstMove : GrimmMove
{
	//static ObjectPool RedBurstPool;

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

	/*[OnIKGAwake]
	static void OnGrimmAwake()
	{
		RedBurstPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.RedBurst, PoolLoadType.Local);
	}*/

	/*void Awake()
	{
		
		//MoveEnabled = true;
	}*/

	public override IEnumerator DoMove()
	{
		startHealth3rdStage = Grimm.MaxHealth - ((Grimm.MaxHealth / 3) * 2);
		healthPercentage = Mathf.Clamp01(Mathf.InverseLerp(endHealth3rdStage, startHealth3rdStage, Grimm.GrimmHealth.Health));
		if (!Grimm.Settings.hardMode || Grimm.BossStage == 1)
		{
			yield break;
		}
		//transform.position = transform.position.With(x: spawnPosition.x, y: spawnPosition.y);
		var newPosition = transform.position.With(x: spawnPosition.x, y: spawnPosition.y);

		/*while (Vector3.Distance(newPosition, Player.Player1.transform.position) < 6f)
		{
			yield return null;
		}*/

		Grimm.FacePlayer(newPosition);

		yield return Grimm.TeleportIn(newPosition, true,"Cape Teleport In");

		//yield return GrimmAnimator.PlayAnimation("Cape Prepare");
		GrimmAnimator.PlayAnimation("Cape Prepare");

		yield return new WaitForSeconds(anticTime / InfernoKingGrimm.InfiniteSpeed);

		GrimmAnimator.speed = InfernoKingGrimm.GetInfiniteSpeed(0.5f);//InfernoKingGrimm.InfiniteSpeed / 2f;
		yield return GrimmAnimator.PlayAnimationTillDone("Cape Open");

		jitterRoutine = StartCoroutine(TransformUtilities.JitterObject(gameObject, JitterAmount, jitterFPS));

		GrimmAnimator.speed = 1f;

		yield return GrimmAnimator.PlayAnimationTillDone("Cape Open Finish");

		//GrimmAnimator.speed = 1f;
		GrimmAnimator.PlayAnimation("Cape Open Loop");

		yield return new WaitForSeconds(startDelay / InfernoKingGrimm.InfiniteSpeed);

		if (Grimm.BossStage == 2)
		{
			yield return FireDirectShots(amountOfDirectShots, timeBetweenDirectShots);
		}
		else
		{
			yield return FireSprayShots(amountOfWaves,shotsPerWave,angleBetweenSprayShots);
		}

		yield return new WaitForSeconds(endDelay / InfernoKingGrimm.InfiniteSpeed);

		StopCoroutine(jitterRoutine);
		jitterRoutine = null;
		yield return GrimmAnimator.PlayAnimationTillDone("Cape Close");
		yield return Grimm.TeleportOut(true, "Cape Teleport Out");

        if (Grimm.FightingInPantheon)
        {
            yield return new WaitForSeconds(0.15f / InfernoKingGrimm.InfiniteSpeed);
        }
        else if ((Grimm.Settings.hardMode && Grimm.BossStage >= 3) || Grimm.FightingInPantheon)
		{
			yield return new WaitForSeconds(0.35f / InfernoKingGrimm.InfiniteSpeed);
		}
		else
		{
			yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
		}


		yield break;
	}

	IEnumerator FireDirectShots(int amountOfShots, float timeBetweenShots)
	{
		for (float i = 0; i < amountOfShots; i++)
		{
			var pitch = Mathf.Lerp(pitchLow, pitchHigh, i / (amountOfShots - 1));
			SpawnHomingBall(VectorUtilities.GetAngleBetween(transform.position, Player.Player1.transform.position), homingBallVelocity * InfernoKingGrimm.GetInfiniteSpeed(1f / 3f,2.0f), homingBallRotationSpeed,true, pitch);
			//Debug.Log("Time Between Shots = " + timeBetweenShots);
			//Debug.Log("Speed = " + InfernoKingGrimm.InfiniteSpeed);
			//Debug.Log("Infinite Speed Divided = " + (InfernoKingGrimm.InfiniteSpeed / 2.5f));
			//Debug.Log("Wait Time = " + (timeBetweenShots / (InfernoKingGrimm.InfiniteSpeed / 2.5f)));
			yield return new WaitForSeconds(timeBetweenShots/* / InfernoKingGrimm.GetInfiniteSpeed(1f / 2.5f)*/);
		}
	}

	int positionMatchCount = 0;

	IEnumerator FireSprayShots(int amountOfWaves, int shotsPerWave, float angleBetweenShots)
	{
		var downAngle = VectorUtilities.VectorToDegrees(Vector2.down);

		var previousPlayerPosition = Player.Player1.transform.position;
		float previousPlayerAngle = GetAngleToPlayer();

		var currentPlayerPosition = Player.Player1.transform.position;
		float currentPlayerAngle = GetAngleToPlayer();

		bool evenWave = true;
		for (float i = 0; i < amountOfWaves; i++)
		{
			previousPlayerPosition = currentPlayerPosition;
			previousPlayerAngle = currentPlayerAngle;

			currentPlayerPosition = Player.Player1.transform.position;
			currentPlayerAngle = GetAngleToPlayer();

			int trueAmountOfShots = shotsPerWave;
			if (evenWave && shotsPerWave % 2 == 1)
			{
				trueAmountOfShots++;
			}

			var angles = VectorUtilities.CalculateSpacedValues(trueAmountOfShots, angleBetweenShots);
			float angleOffset = 0f;

			if (i >= 2 && (Vector3.Distance(currentPlayerPosition, previousPlayerPosition) < 0.02f || currentPlayerPosition.y > transform.position.y + 2f))
			{
				positionMatchCount++;
				if (positionMatchCount >= 3)
				{
					float nearestAngleIndex = 0f;
					float nearestValue = float.PositiveInfinity;
					for (int j = 0; j < angles.GetLength(0); j++)
					{
						var difference = currentPlayerAngle - (angles[j] + downAngle);
						if (Mathf.Abs(difference) <= nearestValue)
						{
							nearestValue = difference;
							nearestAngleIndex = j;
						}
					}
					if (!float.IsInfinity(nearestValue))
					{
						angleOffset = nearestValue;
					}
				}
			}
			else
			{
				positionMatchCount = 0;
			}

			float speed = InfernoKingGrimm.GetInfiniteSpeed(0.5f,1.35f);//InfernoKingGrimm.InfiniteSpeed / 2.5f;

			for (int j = 0; j < trueAmountOfShots; j++)
			{
				//var angle = downAngle + Mathf.Lerp(angleBetweenShots, -angleBetweenShots, j / (trueAmountOfShots - 1));
				var angle = downAngle + angles[j] + angleOffset;
				//WeaverLog.Log("Angle = " + angle);
				//WeaverLog.Log("Lerp Val = " + (j / (trueAmountOfShots - 1)));
				var homingBall = SpawnHomingBall(angle, sprayShotVelocity * speed, sprayShotRotationSpeed, j == trueAmountOfShots - 1, Mathf.Lerp(pitchLow,pitchHigh,i / (amountOfWaves - 1)));
				//var growth = homingBall.gameObject.AddComponent<Growth>();
				var growth = homingBall.GetComponent<Growth>();
				growth.enabled = true;
				homingBall.transform.localScale = Vector3.one * 1.8f;
				growth.GrowthSpeed = 0.30f + (Mathf.Clamp(Mathf.Abs(angles[j] / angleBetweenShots), 0f,3f) * 0.20f * speed);
			}
			evenWave = !evenWave;

			var minTime = Mathf.Lerp(timeBetweenWavesMinRange.x, timeBetweenWavesMinRange.y,healthPercentage);
			var maxTime = Mathf.Lerp(timeBetweenWavesMaxRange.x, timeBetweenWavesMaxRange.y,healthPercentage);

			yield return new WaitForSeconds(Mathf.Lerp(maxTime, minTime, i / (amountOfWaves - 1)) / speed);
		}
	}

	float GetAngleToPlayer()
	{
		//position + homingBallSpawnOffset
		var playerPos = Player.Player1.transform.position;
		var angle = Mathf.Atan2(playerPos.y - (transform.position.y + homingBallSpawnOffset.y), playerPos.x - (transform.position.x + homingBallSpawnOffset.x));
		return angle * Mathf.Rad2Deg;
	}


	public void PlayBurst()
	{
		var instance = Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.RedBurst, transform.position + BurstOffset, Quaternion.identity);
		instance.transform.localScale = new Vector3(burstScale, burstScale, 1f);
	}

	public void PlayScreamSound()
	{
		WeaverAudio.PlayAtPoint(Sounds.GrimmScream, transform.position);
	}

	public void PlayCapeSound()
	{
		WeaverAudio.PlayAtPoint(Sounds.GrimmCapeOpen, transform.position);
	}

	HomingBall SpawnHomingBall(float angle, float velocity, float rotationSpeed, bool playEffects = true, float audioPitch = 1f)
	{
		return SpawnHomingBall(angle, velocity, rotationSpeed, transform.position,playEffects, audioPitch);
	}

	HomingBall SpawnHomingBall(float angle, float velocity, float rotationSpeed, Vector3 position, bool playEffects = true, float audioPitch = 1f)
	{
		HomingBall.GlowScale = GlowScale;
		HomingBall.FirePillarOffset = FirePillarOffset;
		//WeaverLog.Log("Spawning Ball");
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
		base.OnStun();
		if (jitterRoutine != null)
		{
			StopCoroutine(jitterRoutine);
			jitterRoutine = null;
		}
	}
}

