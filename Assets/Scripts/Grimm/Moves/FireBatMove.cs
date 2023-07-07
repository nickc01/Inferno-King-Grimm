using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class FireBatMove : GrimmMove 
{
	[Header("Firebat Settings")]
	[SerializeField]
	float highAngle = 25f;
	[SerializeField]
	float lowAngle = -15f;

	[SerializeField]
	float homingBallSpawnVelocity = 5f;
	[SerializeField]
	float homingBallRotationSpeed = 2f;

	[SerializeField]
	float homingBallVelocity = 50f;

	[SerializeField]
	Vector2 homingBallAngleRange = new Vector2(-10f,60f);
	[SerializeField]
	Vector2 homingBallSpawnVelocityRange = new Vector2(20f,50f);
	[SerializeField]
	float homingBallSpawnRate = 0.2f;
	[SerializeField]
	float homingBallTimePeriod = 2f;
	[SerializeField]
	Vector2 HomingBallPitchRange = new Vector2(0.5f,1.5f);
	[SerializeField]
	float HomingBallVolume = 0.5f;

	[Header("Hard Mode")]
	[SerializeField]
	float hardModePhase2WaitTime = 0.32f;
	[SerializeField]
	Vector2 hardModeBallAngleRange = new Vector2(-10f, 60f);
	[SerializeField]
	Vector2 hardModeBallAngleRangeTop = new Vector2(-10f, 60f);
	[SerializeField]
	Vector2 hardModeBallVelocityRange = new Vector2(20f, 50f);
	[SerializeField]
	float hardModeBallSpawnVelocity = 5f;
	[SerializeField]
	float hardModeBallRotationSpeed = 2f;
	[SerializeField]
	int hardModeBallsPerRound = 4;

	GameObject BalloonFireballShootSound;

	void Awake()
	{
		BalloonFireballShootSound = GetChildGameObject("Balloon Fireball Loop Audio");
	}

	public override IEnumerator DoMove()
	{
		yield return DoMove(true);
	}

	public IEnumerator DoMove(bool teleportIn)
	{
		Vector3 newPosition = transform.position;
		if (teleportIn)
		{
			var playerPos = Player.Player1.transform.position;
			if (playerPos.x <= 88)
			{
				newPosition = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), Grimm.GroundY);
			}
			else
			{
				newPosition = new Vector3(UnityEngine.Random.Range(74.0f, 77.0f), Grimm.GroundY);
			}

			Grimm.FacePlayer(newPosition, true);

			yield return Grimm.TeleportIn(newPosition);
		}
		else
		{
			Grimm.FacePlayer(newPosition, true);
		}

		GrimmAnimator.speed = InfernoKingGrimm.GetInfiniteSpeed(1f,1.5f);
		GrimmAnimator.PlayAnimation("Bat Cast Charge");
		GrimmAnimator.speed = 1f;


		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.PlayAtPoint(Sounds.GrimmCapeOpen, transform.position);

		yield return new WaitForSeconds(0.2f / InfernoKingGrimm.GetInfiniteSpeed(1f,1.5f)/*InfernoKingGrimm.InfiniteSpeed*/);

		GrimmAnimator.speed = InfernoKingGrimm.InfiniteSpeed;
		GrimmAnimator.PlayAnimation("Cast");
		GrimmAnimator.speed = 1f;

		yield return new WaitForSeconds(0.3f / InfernoKingGrimm.GetInfiniteSpeed(1f, 1.5f));

		if (Grimm.BossStage == 1)
		{
			if (Grimm.Settings.hardMode)
			{
				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1f);
				yield return new WaitForSeconds(0.32f);
				Firebat.SendFirebat(Grimm, highAngle / 2, 1.1f);
				yield return new WaitForSeconds(0.32f);
				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1.2f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1.2f);
				yield return new WaitForSeconds(0.32f);
				yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2, 1.3f);
			}
			else
			{
				if (InfernoKingGrimm.GodMode)
				{
					Firebat.SendFirebat(Grimm, highAngle / 1.5f, 1f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1f);

					yield return new WaitForSeconds(0.3f);

					Firebat.SendFirebat(Grimm, lowAngle / 1.5f, 1.2f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, lowAngle, 1.2f);

					/*Firebat.SendFirebat(Grimm, lowAngle, 1f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1f);

					yield return new WaitForSeconds(0.3f);

					Firebat.SendFirebat(Grimm, lowAngle / 2f, 1.2f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2f, 1.2f);*/
				}
				else
				{
					Firebat.SendFirebat(Grimm, lowAngle, 1f);
					yield return new WaitForSeconds(0.1f);
					Firebat.SendFirebat(Grimm, lowAngle / 2f, 1.1f);
					yield return new WaitForSeconds(0.1f);
					Firebat.SendFirebat(Grimm, highAngle / 2f, 1.2f);
					yield return new WaitForSeconds(0.1f);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1.3f);
				}
			}

		}
		else if (Grimm.BossStage == 2)
		{
			if (Grimm.Settings.hardMode)
			{
				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				Firebat.SendFirebat(Grimm, highAngle / 2, 1.05f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1.1f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1.1f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2, 1.15f);

				yield return new WaitForSeconds(0.3f / InfernoKingGrimm.InfiniteSpeed);

				yield return TeleportToOtherSide();

				yield return new WaitForSeconds(0.3f);

				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1.2f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1.2f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				Firebat.SendFirebat(Grimm, highAngle / 2, 1.25f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				Firebat.SendFirebat(Grimm, lowAngle / 1.25f, 1.3f, playSound: false);
				Firebat.SendFirebat(Grimm, highAngle, 1.3f);
				yield return new WaitForSeconds(hardModePhase2WaitTime);
				yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2, 1.35f);
			}
			else
			{
				if (Random.value > 0.5f || InfernoKingGrimm.GodMode)
				{
					Firebat.SendFirebat(Grimm, lowAngle, 1f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1f);

					yield return new WaitForSeconds(0.3f);

					yield return TeleportToOtherSide();

					yield return new WaitForSeconds(0.3f);

					Firebat.SendFirebat(Grimm, lowAngle / 2f, 1.2f, playSound: false);
					yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2f, 1.2f);
				}
				else
				{
					yield return FireballMove();
				}
			}

		}
		else if (Grimm.BossStage >= 3)
		{
			yield return FireballMove();
		}

		yield return new WaitForSeconds(0.3f);

		GrimmAnimator.speed = InfernoKingGrimm.InfiniteSpeed;
		GrimmAnimator.PlayAnimation("Cast Return");
		GrimmAnimator.speed = 1f;

		if (Grimm.Settings.hardMode || Grimm.BossStage == 2 || Random.value < 0.75f)
		{
			yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
		}

		yield return Grimm.TeleportOut();

        if (Grimm.FightingInPantheon)
        {
            yield return new WaitForSeconds(0.15f / InfernoKingGrimm.InfiniteSpeed);
        }
        else if (Grimm.BossStage >= 3 || Grimm.FightingInPantheon)
		{
			yield return new WaitForSeconds(0.3f / InfernoKingGrimm.InfiniteSpeed);
		}
		else
		{
			yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
		}
	}

	IEnumerator TeleportToOtherSide()
	{
		var center = Mathf.Lerp(Grimm.LeftEdge, Grimm.RightEdge, 0.5f);

		float newPositionX = 0f;

		if (Player.Player1.transform.position.x > transform.position.x)
		{
			newPositionX = Grimm.RightEdge - (transform.position.x - Grimm.LeftEdge);
		}
		else
		{
			newPositionX = Grimm.LeftEdge + (Grimm.RightEdge - transform.position.x);
		}

		if (Mathf.Abs(newPositionX - Player.Player1.transform.position.x) < 7f)
		{
			if (Player.Player1.transform.position.x > newPositionX)
			{
				newPositionX = Player.Player1.transform.position.x - 7f;
				if (newPositionX <= Grimm.LeftEdge)
				{
					newPositionX = Player.Player1.transform.position.x + 7f;
				}
			}
			else
			{
				newPositionX = Player.Player1.transform.position.x + 7f;
				if (newPositionX <= Grimm.RightEdge)
				{
					newPositionX = Player.Player1.transform.position.x - 7f;
				}
			}
		}

		var telePosition = new Vector3(newPositionX, Grimm.GroundY, transform.position.z);

		float time = Teleporter.TeleportEntity(gameObject, telePosition, Teleporter.TeleType.Delayed, Color.red);

		yield return new WaitForSeconds(time / 2f);
		Grimm.FacePlayer(telePosition);
		yield return new WaitForSeconds(time / 2f);
	}


	IEnumerator FireballMove()
	{
		var spawnPoint = Grimm.transform.Find("Firebat SpawnPoint").position + new Vector3(0f,1f,0f);
		var spawnRate = homingBallSpawnRate;
		if (!Grimm.Settings.hardMode && Grimm.BossStage >= 3)
		{
			spawnRate *= 0.75f;
		}
		if (!Grimm.Settings.hardMode && InfernoKingGrimm.GodMode)
		{
			spawnRate *= 2.2f;
		}
		float spawnTimer = spawnRate;
		bool top = false;
		int fireBallsShot = 0;

		int fireballsPerRound = hardModeBallsPerRound;

		if (Grimm.Settings.hardMode && (InfernoKingGrimm.GodMode || Grimm.Settings.Infinite))
		{
			fireballsPerRound = 2;
		}

		for (float t = 0; t < homingBallTimePeriod; t += Time.deltaTime)
		{
			yield return null;
			spawnTimer += Time.deltaTime;
			if (spawnTimer >= spawnRate)
			{
				spawnTimer = spawnTimer - spawnRate;
				if (Grimm.Settings.hardMode)
				{
					fireBallsShot++;
					var ball = HomingBall.Fire(Grimm, spawnPoint, Vector2.zero, 0f, true, Mathf.Lerp(HomingBallPitchRange.x, HomingBallPitchRange.y, t / homingBallTimePeriod));
					ball.EnablePhase1 = false;

					float angleModifier = 0f;
					if (Grimm.Settings.Infinite)
					{
						angleModifier = -5f;
					}

					var angle = Random.Range(hardModeBallAngleRange.x, hardModeBallAngleRange.y) + angleModifier;
					if (top)
					{
						angle = Random.Range(hardModeBallAngleRangeTop.x, hardModeBallAngleRangeTop.y);
					}
					ball.Phase2TravelDirection = VectorUtilities.DegreesToVector(angle, Random.Range(hardModeBallVelocityRange.x, hardModeBallVelocityRange.y) * InfernoKingGrimm.GetInfiniteSpeed(0.5f, 2.0f));
					if (Grimm.FaceDirection == GrimmDirection.Left)
					{
						ball.Phase2TravelDirection = ball.Phase2TravelDirection.With(-ball.Phase2TravelDirection.x);
					}
					ball.Phase2Velocity = hardModeBallSpawnVelocity * InfernoKingGrimm.GetInfiniteSpeed(0.5f,2.0f);
					ball.Phase2RotationSpeed = hardModeBallRotationSpeed * InfernoKingGrimm.GetInfiniteSpeed(0.5f,2.0f);
					FirebatFirePillar.Spawn(Grimm);
					GrimmGlow.Create(spawnPoint + new Vector3(0f, 0f, -0.1f));
					if (fireBallsShot >= fireballsPerRound)
					{
						fireBallsShot = 0;
						top = !top;
					}
				}
				else
				{
					var ball = HomingBall.Fire(Grimm, spawnPoint, Random.Range(Grimm.CurrentCycle >= 1 ? 15 : homingBallAngleRange.x, homingBallAngleRange.y), Random.Range(homingBallSpawnVelocityRange.x, homingBallSpawnVelocityRange.y) * InfernoKingGrimm.GetInfiniteSpeed(1f,1.4f), homingBallRotationSpeed, false);
					ball.Phase1Time /= InfernoKingGrimm.GetInfiniteSpeed(2.0f,1.4f * 2f);
					ball.Phase2Velocity = homingBallVelocity * InfernoKingGrimm.GetInfiniteSpeed(2.0f,1.4f * 2f);
					var fireAudio = WeaverAudio.PlayAtPoint(Grimm.Sounds.GrimmBatFire, Grimm.transform.position, HomingBallVolume, AudioChannel.Sound);
					fireAudio.AudioSource.pitch = Mathf.Lerp(HomingBallPitchRange.x, HomingBallPitchRange.y, t / homingBallTimePeriod);
					FirebatFirePillar.Spawn(Grimm);
					GrimmGlow.Create(spawnPoint + new Vector3(0f, 0f, -0.1f));
				}
			}
		}

		yield return new WaitForSeconds(0.1f);

		if (Grimm.Settings.hardMode && Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.35f / InfernoKingGrimm.GetInfiniteSpeed(0.5f)/*(InfernoKingGrimm.InfiniteSpeed / 2f)*/);
		}
		else
		{
			yield return new WaitForSeconds(0.6f / InfernoKingGrimm.GetInfiniteSpeed(0.5f));
		}
	}
}
