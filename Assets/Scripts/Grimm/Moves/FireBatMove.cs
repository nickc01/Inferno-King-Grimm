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

		GrimmAnimator.PlayAnimation("Cast Return");

		if (UnityEngine.Random.Range(0, 1) == 0)
		{
			yield return new WaitForSeconds(0.5f);
		}

		yield return Grimm.TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	IEnumerator SendFirebat(Altitude altitude, float pitch = 1.0f, float speedMultiplier = 1f)
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
		StartCoroutine(SendFirebat(Altitude.High, 1.0f));
		yield return SendFirebat(Altitude.Low, 1.0f);

		GrimmAnimator.PlayAnimation("Cast Return");

		yield return new WaitForSeconds(0.5f);

		yield return Grimm.TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}
}
