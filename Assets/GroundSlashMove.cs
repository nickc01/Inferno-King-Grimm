﻿using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class GroundSlashMove : GrimmMove
{
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

	bool continueToSlash = true;

	Rigidbody2D body;
	ParticleSystem DustGround;
	PolygonCollider2D Slash1;
	PolygonCollider2D Slash2;
	PolygonCollider2D Slash3;
	GameObject AudioScuttle;
	ParticleSystem DustScuttle;
	ParticleSystem DustUppercut;
	ParticleSystem UppercutExplosion;

	void Awake()
	{
		body = GetComponent<Rigidbody2D>();
		DustGround = GetChildObject<ParticleSystem>("Dust Ground");
		Slash1 = GetChildObject<PolygonCollider2D>("Slash1");
		Slash2 = GetChildObject<PolygonCollider2D>("Slash2");
		Slash3 = GetChildObject<PolygonCollider2D>("Slash3");
		DustScuttle = GetChildObject<ParticleSystem>("Dust Scuttle");
		DustUppercut = GetChildObject<ParticleSystem>("Dust Uppercut");
		UppercutExplosion = GetChildObject<ParticleSystem>("Uppercut Explosion");
		AudioScuttle = GetChildGameObject("Audio Scuttle");
	}

	public override IEnumerator DoMove()
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

			if (teleX > Grimm.LeftEdge && teleX < Grimm.RightEdge)
			{
				break;
			}
		}

		transform.position = new Vector3(teleX, Grimm.GroundY, 0f);

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		var raycastDirection = FaceDirection == GrimmDirection.Right ? Vector2.right : Vector2.left;

		var hit = Physics2D.Raycast(transform.position, raycastDirection, 4, LayerMask.GetMask("Terrain"), 0, 0);

		Debug.DrawLine(transform.position, transform.position + ((Vector3)raycastDirection * 4f));

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

		Grimm.FacePlayer();

		WeaverAudio.Play(Sounds.SlashAntic, transform.position);

		GrimmAnimator.PlayAnimation("Slash Antic");

		VoicePlayer.Play(Sounds.SlashAnticVoice);

		yield return new WaitForSeconds(0.5f);

		var xScale = transform.lossyScale.x;

		WeaverAudio.Play(Sounds.GroundSlashAttack, transform.position);

		var speed = xScale * groundSlashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			speed = -speed;
		}

		body.velocity = new Vector2(speed, 0f);

		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, Grimm.LeftEdge, Grimm.RightEdge), () => !Grimm.Stunned);

		DustGround.Play();

		Slash1.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 1");

		Slash1.enabled = false;
		Slash2.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 2");

		Slash2.enabled = false;
		Slash3.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 3");


		Slash3.enabled = false;
		DustGround.Stop();
		GrimmAnimator.PlayAnimation("Slash Recover");

		//This ensures that the current animation is playing
		yield return null;

		yield return CoroutineUtilities.RunForPeriod(GrimmAnimator.GetCurrentAnimationTime(), () =>
		{
			body.velocity = VectorUtilities.Decelerate(body.velocity, new Vector2(0.65f, 0.65f));
		});

		body.velocity = Vector2.zero;
		StopCoroutine(lockRoutine);

		yield return UpperCut();

		yield break;
	}

	IEnumerator GroundEvade()
	{
		continueToSlash = true;
		Grimm.FacePlayer();

		VoicePlayer.Play(Sounds.GrimmEvadeVoice);

		yield return GrimmAnimator.PlayAnimationTillDone("Evade Antic");

		var xScale = transform.lossyScale.x;

		WeaverAudio.Play(Sounds.EvadeSoundEffect, transform.position);

		var speed = xScale * evadeSpeed;

		if (FaceDirection == GrimmDirection.Right)
		{
			speed = -speed;
		}

		body.velocity = new Vector2(speed, 0f);

		GrimmAnimator.PlayAnimation("Evade");

		DustScuttle.Play();
		AudioScuttle.SetActive(true);

		float waitTimer = 0f;
		float waitTime = 0.225f;

		bool horizontal = true;

		do
		{
			yield return null;
			waitTimer += Time.deltaTime;
			if (horizontal && (transform.position.x <= Grimm.LeftEdge || transform.position.x >= Grimm.RightEdge))
			{
				body.velocity = Vector2.zero;
				horizontal = false;
			}
		} while (waitTimer < waitTime);

		DustScuttle.Stop();
		AudioScuttle.SetActive(false);

		body.velocity = new Vector2(0f, 0f);

		yield return GrimmAnimator.PlayAnimationTillDone("Evade End");

		if (Mathf.Abs(transform.position.x - Player.Player1.transform.position.x) > 7f)
		{
			continueToSlash = false;
			yield return GetComponent<FireBatMove>().DoMove();
		}
	}

	IEnumerator UpperCut()
	{
		if (Invisible)
		{
			Grimm.FacePlayer();
			yield return Grimm.TeleportIn();
		}
		body.velocity = Vector2.zero;


		VoicePlayer.Play(Sounds.GrimmUppercutAttackVoice);


		yield return GrimmAnimator.PlayAnimationTillDone("Uppercut Antic");

		DustUppercut.Play();

		GrimmAnimator.PlayAnimation("Uppercut");

		var sf = WeaverAudio.Play(Sounds.UpperCutSoundEffect, transform.position);
		sf.AudioSource.pitch = 1.1f;

		var xScale = transform.lossyScale.x;

		var horizontalSpeed = xScale * uppercutHorizontalSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			horizontalSpeed = -horizontalSpeed;
		}

		body.velocity = new Vector2(horizontalSpeed, uppercutVerticalSpeed);
		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, Grimm.LeftEdge, Grimm.RightEdge), () => !Grimm.Stunned);

		//This function will wait until either the time is up, or the y position is no longer less than or equal to the uppercut height limit
		yield return CoroutineUtilities.RunForPeriod(0.35f, () => transform.position.y <= upperCutHeightLimit);

		transform.position = transform.position.With(y: upperCutHeightLimit);//new Vector3(transform.position.x,upperCutHeightLimit,transform.position.z);

		StopCoroutine(lockRoutine);

		DustUppercut.Stop();

		var explodeSF = WeaverAudio.Play(Sounds.UpperCutExplodeEffect, transform.position);
		explodeSF.AudioSource.pitch = 1.1f;

		body.velocity = Vector2.zero;

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

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

		yield return GrimmAnimator.PlayAnimationTillDone("Uppercut End");

		GrimmCollider.enabled = false;
		Invisible = true;

		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(0.6f);

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

	public override void OnStun()
	{
		body.velocity = Vector2.zero;
		DustGround.Stop();
		Slash1.enabled = false;
		Slash2.enabled = false;
		Slash3.enabled = false;
		DustScuttle.Stop();
		AudioScuttle.SetActive(false);
		DustUppercut.Stop();
		UppercutExplosion.Stop();
	}
}