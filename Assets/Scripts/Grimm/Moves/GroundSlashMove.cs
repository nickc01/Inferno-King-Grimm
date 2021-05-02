using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
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

	[Header("Hard Mode")]
	float extraWaitTime = 0.15f;

	//Rigidbody2D body;
	ParticleSystem DustGround;
	PolygonCollider2D Slash1;
	PolygonCollider2D Slash2;
	PolygonCollider2D Slash3;
	GameObject AudioScuttle;
	ParticleSystem DustScuttle;
	ParticleSystem DustUppercut;
	ParticleSystem UppercutExplosion;

	GrimmDirection slashDirection = GrimmDirection.Left;

	void Awake()
	{
		//body = GetComponent<Rigidbody2D>();
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

		WeaverAudio.PlayAtPoint(Sounds.SlashAntic, transform.position);
		VoicePlayer.Play(Sounds.SlashAnticVoice);

		yield return DoSlash();

		//WeaverLog.Log("Grimm Number " + InfernoKingGrimm.GrimmsFighting.IndexOf(Grimm) + " visibility = " + Grimm.GetComponent<SpriteRenderer>().enabled);

		//var sprite = GetComponent<SpriteRenderer>().sprite;
		/*if (sprite == null)
		{
			WeaverLog.Log("Current Sprite = " + sprite.name);
		}
		else
		{
			WeaverLog.Log("Current Sprite = null");
		}*/

		if (Grimm.Settings.hardMode && Grimm.BossStage >= 2)
		{
			var playerPositionOld = Player.Player1.transform.position.x;
			WeaverAudio.PlayAtPoint(Sounds.SlashAntic, transform.position);
			StartCoroutine(Teleport(playerPositionOld));
			yield return DoSlash();
		}
		/*Grimm.FacePlayer();

		GrimmAnimator.PlayAnimation("Slash Antic");

		var playerPositionOld = Player.Player1.transform.position.x;

		yield return new WaitForSeconds(0.5f);

		if (Grimm.Settings.hardMode)
		{
			if (Random.value > 0.5f)
			{
				yield return Teleport(playerPositionOld);
			}
		}
		else
		{
			if (Grimm.BossStage >= 2)
			{
				yield return Teleport(playerPositionOld);
			}
		}


		var xScale = transform.lossyScale.x;

		WeaverAudio.Play(Sounds.GroundSlashAttack, transform.position);

		var speed = xScale * groundSlashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			speed = -speed;
		}

		Grimm.Velocity = new Vector2(speed, 0f);

		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, Grimm.LeftEdge, Grimm.RightEdge), () => !Grimm.Stunned);

		DustGround.Play();

		AlignSlashes();

		Slash1.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 1");

		Slash1.enabled = false;
		Slash2.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 2");

		Slash2.enabled = false;
		Slash3.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 3");


		Slash3.enabled = false;

		ResetSlashes();

		DustGround.Stop();
		GrimmAnimator.PlayAnimation("Slash Recover");

		//This ensures that the current animation is playing
		yield return null;

		yield return CoroutineUtilities.RunForPeriod(GrimmAnimator.GetCurrentAnimationTime(), () =>
		{
			Grimm.Velocity = VectorUtilities.Decelerate(Grimm.Velocity, new Vector2(0.65f, 0.65f));
		});

		Grimm.Velocity = Vector2.zero;
		StopCoroutine(lockRoutine);*/

		yield return UpperCut();

		yield break;
	}

	IEnumerator DoSlash()
	{
		Grimm.FacePlayer();

		GrimmAnimator.PlayAnimation("Slash Antic");

		var playerPositionOld = Player.Player1.transform.position.x;

		yield return new WaitForSeconds(0.5f);

		if (Grimm.Settings.hardMode && Grimm.BossStage == 2)
		{
			yield return new WaitForSeconds(extraWaitTime);
		}

		if (Grimm.Settings.hardMode)
		{
			if (Grimm.BossStage == 1)
			{
				yield return Teleport(playerPositionOld);
			}
		}
		else
		{
			if (Grimm.BossStage >= 2)
			{
				yield return Teleport(playerPositionOld);
			}
		}


		var xScale = transform.lossyScale.x;

		WeaverAudio.PlayAtPoint(Sounds.GroundSlashAttack, transform.position);

		var speed = xScale * groundSlashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			speed = -speed;
		}

		Grimm.Velocity = new Vector2(speed, 0f);

		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, Grimm.LeftEdge, Grimm.RightEdge), () => !Grimm.Stunned);

		DustGround.Play();

		AlignSlashes();

		Slash1.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 1");

		Slash1.enabled = false;
		Slash2.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 2");

		Slash2.enabled = false;
		Slash3.enabled = true;

		yield return GrimmAnimator.PlayAnimationTillDone("Slash 3");


		Slash3.enabled = false;

		ResetSlashes();

		DustGround.Stop();
		GrimmAnimator.PlayAnimation("Slash Recover");

		//This ensures that the current animation is playing
		yield return null;

		yield return CoroutineUtilities.RunForPeriod(GrimmAnimator.GetCurrentAnimationTime(), () =>
		{
			Grimm.Velocity = VectorUtilities.Decelerate(Grimm.Velocity, new Vector2(0.65f * Time.deltaTime, 0.65f * Time.deltaTime));
		});

		Grimm.Velocity = Vector2.zero;
		StopCoroutine(lockRoutine);
	}

	void AlignSlashes()
	{
		slashDirection = Grimm.FaceDirection;

		if (slashDirection == GrimmDirection.Right)
		{
			Slash1.transform.SetXLocalScale(-Slash1.transform.localScale.x);
			Slash2.transform.SetXLocalScale(-Slash2.transform.localScale.x);
			Slash3.transform.SetXLocalScale(-Slash3.transform.localScale.x);

			Slash1.transform.SetXLocalPosition(-Slash1.transform.localPosition.x);
			Slash2.transform.SetXLocalPosition(-Slash2.transform.localPosition.x);
			Slash3.transform.SetXLocalPosition(-Slash3.transform.localPosition.x);
		}
	}

	void ResetSlashes()
	{
		if (slashDirection == GrimmDirection.Right)
		{
			Slash1.transform.SetXLocalScale(-Slash1.transform.localScale.x);
			Slash2.transform.SetXLocalScale(-Slash2.transform.localScale.x);
			Slash3.transform.SetXLocalScale(-Slash3.transform.localScale.x);

			Slash1.transform.SetXLocalPosition(-Slash1.transform.localPosition.x);
			Slash2.transform.SetXLocalPosition(-Slash2.transform.localPosition.x);
			Slash3.transform.SetXLocalPosition(-Slash3.transform.localPosition.x);

			slashDirection = GrimmDirection.Left;
		}
	}

	IEnumerator Teleport(float originalPlayerX)
	{
		float teleX = 0f;
		while (true)
		{
			var heroX = Player.Player1.transform.position.x;
			var teleAdder = Random.Range(9.5f, 11f);

			if (heroX > originalPlayerX)
			{
				teleX = heroX + teleAdder;
				if (teleX > Grimm.RightEdge)
				{
					teleX = heroX - teleAdder;
				}
			}
			else
			{
				teleX = heroX - teleAdder;
				if (teleX < Grimm.LeftEdge)
				{
					teleX = heroX + teleAdder;
				}
			}

			if (teleX > Grimm.LeftEdge && teleX < Grimm.RightEdge)
			{
				break;
			}
		}

		//transform.position = new Vector3(teleX, Grimm.GroundY, 0f);
		var telePosition = new Vector3(teleX, Grimm.GroundY, 0f);
		var teleTime = Teleporter.TeleportEntity(gameObject, telePosition, Teleporter.TeleType.Delayed, Color.red);

		yield return new WaitForSeconds(teleTime / 2f);
		Grimm.FacePlayer(telePosition);
		yield return new WaitForSeconds(teleTime / 2f);

		yield return new WaitForSeconds(0.4f);
	}

	IEnumerator GroundEvade()
	{
		continueToSlash = true;
		Grimm.FacePlayer();

		VoicePlayer.Play(Sounds.GrimmEvadeVoice);

		yield return GrimmAnimator.PlayAnimationTillDone("Evade Antic");

		var xScale = transform.lossyScale.x;

		WeaverAudio.PlayAtPoint(Sounds.EvadeSoundEffect, transform.position);

		var speed = xScale * evadeSpeed;

		if (FaceDirection == GrimmDirection.Right)
		{
			speed = -speed;
		}

		Grimm.Velocity = new Vector2(speed, 0f);

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
				Grimm.Velocity = Vector2.zero;
				horizontal = false;
			}
		} while (waitTimer < waitTime);

		DustScuttle.Stop();
		AudioScuttle.SetActive(false);

		Grimm.Velocity = new Vector2(0f, 0f);

		yield return GrimmAnimator.PlayAnimationTillDone("Evade End");

		if (Mathf.Abs(transform.position.x - Player.Player1.transform.position.x) > 7f)
		{
			continueToSlash = false;
			yield return GetComponent<FireBatMove>().DoMove();
		}
	}

	public IEnumerator UpperCut(bool playVoice = true)
	{
		if (Invisible)
		{
			Grimm.FacePlayer();
			yield return Grimm.TeleportIn();
		}
		Grimm.Velocity = Vector2.zero;

		if (playVoice)
		{
			VoicePlayer.Play(Sounds.GrimmUppercutAttackVoice);
			//yield return GrimmAnimator.PlayAnimationTillDone("Uppercut Antic");
			yield return GrimmAnimator.PlayAnimationTillDone("Uppercut Antic");
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
		}


		DustUppercut.Play();

		GrimmAnimator.PlayAnimation("Uppercut");

		var sf = WeaverAudio.PlayAtPoint(Sounds.UpperCutSoundEffect, transform.position);
		sf.AudioSource.pitch = 1.1f;

		var xScale = transform.lossyScale.x;

		var horizontalSpeed = xScale * uppercutHorizontalSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			horizontalSpeed = -horizontalSpeed;
		}

		Grimm.Velocity = new Vector2(horizontalSpeed, uppercutVerticalSpeed);
		var lockRoutine = CoroutineUtilities.RunCoroutineWhile(this, HorizontalLock(transform, Grimm.LeftEdge, Grimm.RightEdge), () => !Grimm.Stunned);

		//This function will wait until either the time is up, or the y position is no longer less than or equal to the uppercut height limit
		yield return CoroutineUtilities.RunForPeriod(0.35f, () => transform.position.y <= upperCutHeightLimit);

		transform.position = transform.position.With(y: upperCutHeightLimit);//new Vector3(transform.position.x,upperCutHeightLimit,transform.position.z);

		StopCoroutine(lockRoutine);

		DustUppercut.Stop();

		var explodeSF = WeaverAudio.PlayAtPoint(Sounds.UpperCutExplodeEffect, transform.position);
		explodeSF.AudioSource.pitch = 1.1f;

		Grimm.Velocity = Vector2.zero;

		CameraShaker.Instance.Shake(ShakeType.AverageShake);

		UppercutExplosion.Play();

		var fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(0f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(12f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(-12f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(24f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(-24f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(36f, 1f);
		fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
		fireBall.RigidBody.velocity = new Vector2(-36f, 1f);

		yield return GrimmAnimator.PlayAnimationTillDone("Uppercut End");

		GrimmCollider.enabled = false;
		Invisible = true;

		yield return new WaitForSeconds(1f);
		if (Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}

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
		Grimm.Velocity = Vector2.zero;
		DustGround.Stop();
		Slash1.enabled = false;
		Slash2.enabled = false;
		Slash3.enabled = false;
		ResetSlashes();
		DustScuttle.Stop();
		AudioScuttle.SetActive(false);
		DustUppercut.Stop();
		UppercutExplosion.Stop();
		GetComponent<FireBatMove>().OnStun();
	}

	public override void OnDeath()
	{
		//base.OnDeath();
		OnStun();
		GetComponent<FireBatMove>().OnDeath();
	}

	public override void OnCancel()
	{
		OnStun();
		GetComponent<FireBatMove>().OnCancel();
	}
}
