using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
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

	public override IEnumerator DoMove()
	{
		if (Grimm.Invisible)
		{
			var playerPos = Player.Player1.transform.position;
			if (playerPos.x <= 88)
			{
				transform.position = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), Grimm.GroundY);
			}
			else
			{
				transform.position = new Vector3(UnityEngine.Random.Range(74.0f, 77.0f), Grimm.GroundY);
			}
		}

		Grimm.FacePlayer(true);

		yield return Grimm.TeleportIn();

		GrimmAnimator.PlayAnimation("Bat Cast Charge");


		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);

		yield return new WaitForSeconds(0.2f);

		GrimmAnimator.PlayAnimation("Cast");

		yield return new WaitForSeconds(0.3f);

		if (Grimm.BossStage == 1)
		{
			Firebat.SendFirebat(Grimm, highAngle, 1f);
			yield return Firebat.SendFirebatAsync (Grimm, highAngle / 2f, 1f);

			yield return new WaitForSeconds(0.3f);

			Firebat.SendFirebat(Grimm, lowAngle, 1.2f);
			yield return Firebat.SendFirebatAsync(Grimm, lowAngle / 2f, 1.2f);

			yield return new WaitForSeconds(0.3f);

			Firebat.SendFirebat(Grimm, lowAngle, 1.3f);
			yield return Firebat.SendFirebatAsync(Grimm, lowAngle / 2f, 1.3f);
		}
		else if (Grimm.BossStage == 2)
		{
			//Debugger.Log("FIRING HOMING ATTACK");
			if (Random.value > 0.5f)
			{
				Firebat.SendFirebat(Grimm, lowAngle, 1f);
				yield return new WaitForSeconds(0.1f);
				Firebat.SendFirebat(Grimm, lowAngle / 2f, 1.1f);
				yield return new WaitForSeconds(0.1f);
				Firebat.SendFirebat(Grimm, highAngle / 2f, 1.2f);
				yield return new WaitForSeconds(0.1f);
				yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1.3f);
			}
			else
			{
				var ball = HomingBall.Fire(Grimm, transform.position, -10f, homingBallSpawnVelocity,homingBallRotationSpeed, true);
				ball.Velocity = homingBallVelocity;
				ball = HomingBall.Fire(Grimm, transform.position, 5f, homingBallSpawnVelocity, homingBallRotationSpeed, false);
				ball.Velocity = homingBallVelocity;
				ball = HomingBall.Fire(Grimm, transform.position, 20f, homingBallSpawnVelocity, homingBallRotationSpeed, false);
				ball.Velocity = homingBallVelocity;
				ball = HomingBall.Fire(Grimm, transform.position, 35f, homingBallSpawnVelocity, homingBallRotationSpeed, false);
				ball.Velocity = homingBallVelocity;
				ball = HomingBall.Fire(Grimm, transform.position, 50f, homingBallSpawnVelocity, homingBallRotationSpeed, false);
				ball.Velocity = homingBallVelocity;

				yield return new WaitForSeconds(0.3f);
			}
		}
		else if (Grimm.BossStage >= 3)
		{
			Debugger.Log("Fire Bat A");
			Firebat.SendFirebat(Grimm, lowAngle, 1f);
			yield return Firebat.SendFirebatAsync(Grimm, highAngle, 1f);

			yield return new WaitForSeconds(0.3f);

			var center = Mathf.Lerp(Grimm.LeftEdge,Grimm.RightEdge,0.5f);

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
				/*newPositionX = Player.Player1.transform.position.x - 7f;
				if (newPositionX <= Grimm.LeftEdge)
				{
					newPositionX = Player.Player1.transform.position.x + 7f;
				}*/
			}

			var telePosition = new Vector3(newPositionX, Grimm.GroundY, transform.position.z);

			float time = Teleporter.TeleportEntity(gameObject, telePosition , Teleporter.TeleType.Delayed, Color.red);

			yield return new WaitForSeconds(time / 2f);
			Grimm.FacePlayer(telePosition);
			yield return new WaitForSeconds(time / 2f);

			yield return new WaitForSeconds(0.3f);

			Firebat.SendFirebat(Grimm, lowAngle / 2f, 1.2f);
			yield return Firebat.SendFirebatAsync(Grimm, highAngle / 2f, 1.2f);
		}

		/*yield return SendFirebatAsync(Altitude.High, 1.0f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebatAsync(Altitude.Low, 1.1f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebatAsync(Altitude.High, 1.2f);
		if (!PlayerInFront())
		{
			yield return BackupFirebatMove();
			yield break;
		}
		yield return SendFirebatAsync(Altitude.Low, 1.3f, 0.75f);*/

		yield return new WaitForSeconds(0.3f);

		GrimmAnimator.PlayAnimation("Cast Return");

		if (Grimm.BossStage == 2 || Random.value < 0.75f)
		{
			yield return new WaitForSeconds(0.6f);
		}

		yield return Grimm.TeleportOut();

		if (Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}
	}

	/*IEnumerator SendFirebatAsync(Altitude altitude, float pitch = 1.0f, float speedMultiplier = 1f)
	{
		var fireBatVelocity = transform.localScale.x * 25f * speedMultiplier;

		Firebat.Spawn(altitude == Altitude.High ? highAngle : lowAngle, fireBatVelocity, Grimm);
		FirebatFirePillar.Spawn(Grimm);

		var fireAudio = WeaverAudio.Play(Sounds.GrimmBatFire, transform.position, 1.0f, AudioChannel.Sound);
		Debug.Log("C");
		fireAudio.AudioSource.pitch = pitch;

		yield return new WaitForSeconds(0.3f);

		yield break;
	}

	void SendFirebat(Altitude altitude, float pitch = 1.0f, float speedMultiplier = 1f)
	{
		StartCoroutine(SendFirebatAsync(altitude, pitch, speedMultiplier));
	}*/

	IEnumerator BackupFirebatMove()
	{
		yield return Grimm.TeleportOut();

		var playerPos = Player.Player1.transform.position;
		if (playerPos.x <= 88)
		{
			transform.position = new Vector3(UnityEngine.Random.Range(96.0f, 99.0f), Grimm.GroundY);
		}
		else
		{
			transform.position = new Vector3(UnityEngine.Random.Range(74.0f, 77.0f), Grimm.GroundY);
		}

		Grimm.FacePlayer(true);

		yield return Grimm.TeleportIn();

		VoicePlayer.Play(Sounds.GrimmBeginBatCastVoice);
		WeaverAudio.Play(Sounds.GrimmCapeOpen, transform.position);

		GrimmAnimator.PlayAnimation("Bat Cast Charge");

		yield return new WaitForSeconds(0.2f);

		GrimmAnimator.PlayAnimation("Cast");

		yield return new WaitForSeconds(0.3f);
		Firebat.SendFirebat(Grimm,highAngle, 1.0f);
		yield return Firebat.SendFirebatAsync(Grimm,lowAngle, 1.0f);

		GrimmAnimator.PlayAnimation("Cast Return");

		yield return new WaitForSeconds(0.5f);

		yield return Grimm.TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}
}
