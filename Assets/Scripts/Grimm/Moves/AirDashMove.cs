using Assets.Scripts;
using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;
using Random = UnityEngine.Random;

public class AirDashMove : GrimmMove
{
	/*static ObjectPool AirDashEffectPool;
	static ObjectPool FlameTrailEffectsPool;
	static ObjectPool SlamEffectPool;
	static ObjectPool GroundDashEffectPool;
	static ObjectPool DustGroundEffectPool;*/

	[Header("Air Dash Settings")]
	[SerializeField]
	float airDashSpeed = 55f;
	[SerializeField]
	float flameSpawnRate = 0.025f;
	[SerializeField]
	Vector3 flameSpawnOffset = new Vector3(0f, -1f, 0.001f);
	[SerializeField]
	float groundDashSpeed = 58f;
	[SerializeField]
	float groundDashTime = 0.25f;
	[SerializeField]
	float aheadOfTimeMovement = 0.6f;
	[SerializeField]
	float flankTime = 0.4f;

	[Header("Wait Times")]
	[SerializeField]
	float stageOneWaitTime = 0.6f;
	[SerializeField]
	float stageThreeWaitTime = 0.400f;
	[SerializeField]
	float stageTwoHardModeWaitTime = 0.285f;

	DamageHero damager;
	int previousDamageAmount = 0;

	[SerializeField]
	[Tooltip("How much lower should the lowest fireballs be in the hard mode ground pound move")]
	float loweringOffset = 0f;

	/*[Serializable]
	class GroundPoundHardMode
	{

	}*/
	[Header("Ground Pound Hard Mode")]
	[SerializeField]
	public int hardModeFireballs = 6;
	public float hardModeAngleBetweenFireballs = 37f;
	public float hardModeFireballVelocity = 28f;
	public float hardModeFireballGapAngle = 18f;
	//GroundPoundHardMode groundPoundHardMode;

	[Header("God Mode")]
	[SerializeField]
	float godModeOffset = 2.5f;

	//Rigidbody2D body;
	PolygonCollider2D DashSpike;
	GroundSlashMove groundSlash;
	ParticleSystem UppercutExplosion;

	/*class Hooks : GrimmHooks
	{
		public override void OnGrimmAwake(InfernoKingGrimm grimm)
		{
			AirDashEffectPool = new Pool(grimm.Prefabs.AirDashEffect, PoolType.Local);
			FlameTrailEffectsPool = new Pool(grimm.Prefabs.FlameTrailEffects, PoolType.Local);
			SlamEffectPool = new Pool(grimm.Prefabs.SlamEffect, PoolType.Local);
			GroundDashEffectPool = new Pool(grimm.Prefabs.GroundDashEffect, PoolType.Local);
			DustGroundEffectPool = new Pool(grimm.Prefabs.DustGroundEffect, PoolType.Local);
		}
	}*/

	/*[OnIKGAwake]
	static void OnGrimmAwake()
	{
		AirDashEffectPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.AirDashEffect, PoolLoadType.Local);
		FlameTrailEffectsPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.FlameTrailEffects, PoolLoadType.Local);
		SlamEffectPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.SlamEffect, PoolLoadType.Local);
		GroundDashEffectPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.GroundDashEffect, PoolLoadType.Local);
		DustGroundEffectPool = new ObjectPool(InfernoKingGrimm.Instance.Prefabs.DustGroundEffect, PoolLoadType.Local);
	}*/

	void Awake()
	{
		//body = GetComponent<Rigidbody2D>();
		DashSpike = GetChildObject<PolygonCollider2D>("Dash Spike");
		UppercutExplosion = GetChildObject<ParticleSystem>("Uppercut Explosion");
		groundSlash = GetComponent<GroundSlashMove>();
		damager = GetComponent<DamageHero>();
	}

	public override IEnumerator DoMove()
	{
		if (Grimm.Settings.hardMode || Grimm.BossStage >= 2)
		{
			yield return GroundPoundMove();
			yield break;
		}
		yield return NormalMove();
	}

	IEnumerator NormalMove()
	{
		float teleX = float.PositiveInfinity;
		do
		{
			var playerPos = Player.Player1.transform.position;

			var teleAdder = Random.Range(7.5f, 9f);

			if (Random.value > 0.5f)
			{
				teleAdder = -teleAdder;
			}

			teleX = teleAdder + playerPos.x;

			//Debugger.Log("Tele X = " + teleX);

			if (teleX < 75f || teleX > 99f)
			{
				teleX = float.PositiveInfinity;
				continue;
			}

			yield return null;
		}
		while (teleX == float.PositiveInfinity);

		//Debugger.Log("B");

		transform.position = new Vector3(teleX, 15f, 0f);

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		//Debugger.Log("Angle To Player = " + transform.position.GetAngleBetween(Player.Player1.transform.position));

		var velocityAngle = Mathf.Clamp(transform.position.GetAngleBetween(Player.Player1.transform.position), -135f, -45f);
		var spriteAngle = velocityAngle + 90f;

		//Debugger.Log("Angle = " + velocityAngle);
		//WeaverAudio.Play(Sounds.AirDashAntic, transform.position, 1, AudioChannel.Sound);

		//VoicePlayer.Play();

		yield return GrimmAnimator.PlayAnimationTillDone("Air Dash Antic");

		GrimmAnimator.PlayAnimation("Air Dash");


		Grimm.Velocity = VectorUtilities.DegreesToVector(velocityAngle, airDashSpeed);
		transform.eulerAngles = new Vector3(0f, 0f, spriteAngle);



		WeaverAudio.PlayAtPoint(Sounds.AirDash, transform.position);



		var effect = Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.AirDashEffect, transform, false);
		effect.transform.parent = null;


		float fireTimer = 0f;

		bool vertical = false;

		do
		{
			yield return null;
			fireTimer += Time.deltaTime;
			if (fireTimer >= flameSpawnRate)
			{
				fireTimer -= flameSpawnRate;
				Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (!vertical && (transform.position.x <= Grimm.LeftEdge || transform.position.x >= Grimm.RightEdge))
			{
				Grimm.Velocity = VectorUtilities.DegreesToVector(-90f, airDashSpeed);
				transform.eulerAngles = Vector3.zero;
				vertical = true;
			}
		} while (transform.position.y > Grimm.GroundY);


		Grimm.Velocity = Vector2.zero;

		transform.position = new Vector3(transform.position.x, Grimm.GroundY, transform.position.z);

		GrimmAnimator.PlayAnimation("Ground Dash Antic");

		DashSpike.enabled = true;
		GrimmCollider.enabled = false;

		var landPlayer = WeaverAudio.PlayAtPoint(Sounds.LandSound, transform.position);
		landPlayer.AudioSource.pitch = 0.9f;

		//TODO : Make Camera Shake
		CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

		Grimm.FacePlayer();
		transform.rotation = Quaternion.identity;

		Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.SlamEffect, transform.position + Prefabs.SlamEffect.transform.position, Quaternion.identity);

		yield return new WaitForSeconds(0.37f);

		WeaverAudio.PlayAtPoint(Sounds.GroundDash, transform.position);

		var groundEffect = Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.GroundDashEffect, transform.position + Prefabs.GroundDashEffect.transform.position, Quaternion.identity);
		var dashSpeed = transform.localScale.x * groundDashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			groundEffect.GetComponent<SpriteRenderer>().flipX = true;
			groundEffect.transform.position -= Prefabs.GroundDashEffect.transform.position * 2f;
			dashSpeed = -dashSpeed;
		}

		Grimm.Velocity = new Vector2(dashSpeed, 0f);

		GrimmAnimator.PlayAnimation("G Dash");

		var dustEffect = Pooling.Instantiate<ParticleSystem>(InfernoKingGrimm.MainGrimm.Prefabs.DustGroundEffect, transform.position + Prefabs.DustGroundEffect.transform.position, Prefabs.DustGroundEffect.transform.rotation);
		var mainParticles = dustEffect.main;

		mainParticles.startLifetime = groundDashTime;
		Destroy(dustEffect.gameObject, groundDashTime + 0.5f);


		//dustEffect.main.startLifetime = groundDashTime;

		fireTimer = 0f;
		float waitTimer = 0f;

		bool horizontal = true;

		do
		{
			yield return null;
			fireTimer += Time.deltaTime;
			waitTimer += Time.deltaTime;
			if (fireTimer >= flameSpawnRate)
			{
				fireTimer -= flameSpawnRate;
				Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (horizontal && (transform.position.x <= Grimm.LeftEdge || transform.position.x >= Grimm.RightEdge))
			{
				Grimm.Velocity = Vector2.zero;//VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				horizontal = false;
			}
		} while (waitTimer < groundDashTime);

		GrimmAnimator.PlayAnimation("Ground Dash Antic");

		waitTimer = 0f;

		dustEffect.Stop();

		do
		{
			yield return null;
			Grimm.Velocity = VectorUtilities.Decelerate(Grimm.Velocity, new Vector2(0.75f, float.NaN));
			waitTimer += Time.deltaTime;
		} while (waitTimer < 0.33f);

		Grimm.Velocity = Vector2.zero;

		DashSpike.enabled = false;
		GrimmCollider.enabled = true;

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

	IEnumerator GroundPoundMove()
	{
		var disableDamager = Grimm.Settings.hardMode && Grimm.BossStage >= 2;
		yield return GroundPoundAlt(disableDamager);
		if (Grimm.BossStage >= 3 || (Grimm.BossStage == 2 && Grimm.Settings.hardMode))
		{
			if (!InfernoKingGrimm.GodMode)
			{
				yield return GroundPoundAlt(disableDamager);
				yield return GroundPoundAlt(disableDamager);
			}
		}

		yield return Grimm.TeleportOut();

		if (Grimm.Settings.hardMode && Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(0.35f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}

	}

	IEnumerator GroundPoundAlt(bool disableDamager = false)
	{
		yield return GroundPoundTeleIn(0f);

		yield return GrimmAnimator.PlayAnimationTillDone("Air Dash Antic");

		GrimmAnimator.PlayAnimation("Air Dash");

		Grimm.Velocity = Vector2.down * airDashSpeed * 1.25f;

		WeaverAudio.PlayAtPoint(Sounds.AirDash, transform.position);

		var effect = Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.AirDashEffect, transform, false);
		effect.transform.parent = null;

		//float timer = 0f;
		float fireTimer = 0f;
		do
		{
			yield return null;
			fireTimer += Time.deltaTime;
			if (fireTimer >= flameSpawnRate)
			{
				fireTimer -= flameSpawnRate;
				Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
		} while (transform.position.y > Grimm.GroundY);

		//}


		Grimm.Velocity = Vector2.zero;

		transform.position = new Vector3(transform.position.x, Grimm.GroundY, transform.position.z);

		GrimmAnimator.PlayAnimation("Ground Dash Antic");

		var landPlayer = WeaverAudio.PlayAtPoint(Sounds.LandSound, transform.position);
		landPlayer.AudioSource.pitch = 0.9f;

		if (Grimm.Settings.hardMode)
		{
			yield return GroundPoundEffectsHarder(hardModeFireballs, hardModeAngleBetweenFireballs, hardModeFireballVelocity);
		}
		else
		{
			yield return GroundPoundSlamEffects(5, 14f, 20f);
		}

		yield return new WaitForSeconds(0.2f);

		if (disableDamager)
		{
			previousDamageAmount = damager.DamageDealt;
			damager.DamageDealt = 0;
		}

		if (Grimm.Settings.hardMode && Grimm.BossStage >= 2)
		{
			yield return new WaitForSeconds(stageTwoHardModeWaitTime);
		}
		else if (Grimm.BossStage >= 3)
		{
			yield return new WaitForSeconds(stageThreeWaitTime);
		}
		else
		{
			yield return new WaitForSeconds(stageOneWaitTime);
		}

		if (disableDamager)
		{
			damager.DamageDealt = previousDamageAmount;
			previousDamageAmount = 0;
		}
	}

	IEnumerator GroundPoundEffectsHarder(int amountOfFireballs, float angleBetweenFireballs, float velocity)
	{
		CameraShaker.Instance.Shake(ShakeType.BigShake);

		Grimm.FacePlayer();

		Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.SlamEffect, transform.position + Prefabs.SlamEffect.transform.position, Quaternion.identity);

		UppercutExplosion.Play();

		var upAngle = VectorUtilities.VectorToDegrees(Vector2.up);

		List<Rigidbody2D> Fireballs = new List<Rigidbody2D>();

		foreach (var value in VectorUtilities.CalculateSpacedValues(amountOfFireballs, angleBetweenFireballs))
		{
			var fireBall = UppercutFireball.Create(transform.position);
			fireBall.RigidBody.velocity = VectorUtilities.DegreesToVector(upAngle + value, velocity);
			Fireballs.Add(fireBall.RigidBody);
		}

		if (Fireballs.Count >= 4)
		{
			CreateFireballGap(Fireballs[2],hardModeFireballGapAngle,upAngle);
			CreateFireballGap(Fireballs[3], hardModeFireballGapAngle, upAngle);

			var selectedFireball = Fireballs[Fireballs.Count - 1];

			if (InfernoKingGrimm.GodMode)
			{
				selectedFireball.transform.SetYPosition(selectedFireball.transform.GetYPosition() - (loweringOffset / 2f));
			}

			var fireBall = UppercutFireball.Create(transform.position.With(y: transform.position.y - loweringOffset));
			fireBall.RigidBody.velocity = selectedFireball.velocity;

			selectedFireball = Fireballs[Fireballs.Count - 2];

			if (InfernoKingGrimm.GodMode)
			{
				selectedFireball.transform.SetYPosition(selectedFireball.transform.GetYPosition() - (loweringOffset / 2f));
			}

			fireBall = UppercutFireball.Create(transform.position.With(y: transform.position.y - loweringOffset), Quaternion.identity);
			fireBall.RigidBody.velocity = selectedFireball.velocity;
		}

		var explodeSF = WeaverAudio.PlayAtPoint(Sounds.UpperCutExplodeEffect, transform.position);
		explodeSF.AudioSource.pitch = 1.1f;

		yield break;
	}

	void CreateFireballGap(Rigidbody2D fireball, float gapAngle, float upAngle)
	{
		float degrees = VectorUtilities.VectorToDegrees(fireball.velocity);
		float magnitude = fireball.velocity.magnitude;

		if (degrees - upAngle > 0)
		{
			degrees -= gapAngle;
		}
		else
		{
			degrees += gapAngle;
		}

		fireball.velocity = VectorUtilities.DegreesToVector(degrees, magnitude);
	}

	IEnumerator GroundPoundSlamEffects(int fireballs, float maxHorizontalSpan, float height)
	{
		CameraShaker.Instance.Shake(ShakeType.BigShake);

		Grimm.FacePlayer();

		Pooling.Instantiate(InfernoKingGrimm.MainGrimm.Prefabs.SlamEffect, transform.position + Prefabs.SlamEffect.transform.position, Quaternion.identity);

		UppercutExplosion.Play();
		foreach (var value in VectorUtilities.CalculateSpacedValues(fireballs, 7f))
		{
			var fireBall = UppercutFireball.Create(transform.position, Quaternion.identity);
			fireBall.RigidBody.velocity = new Vector2(value, height);
		}

		var explodeSF = WeaverAudio.PlayAtPoint(Sounds.UpperCutExplodeEffect, transform.position);
		explodeSF.AudioSource.pitch = 1.1f;

		yield break;
	}

	IEnumerator GroundPoundTeleIn(float aheadOfTimeMovement)
	{
		Grimm.Velocity = Vector2.zero;

		var previousPlayerPos = Player.Player1.transform.position;

		yield return null;

		var newPlayerPos = Player.Player1.transform.position;

		var playerPosDiff = newPlayerPos - previousPlayerPos;

		var nextPlayerPos = Player.Player1.transform.position + ((playerPosDiff / Time.deltaTime) * aheadOfTimeMovement);


		var newPosition = new Vector3(nextPlayerPos.x, 19f, 0f);

		if (InfernoKingGrimm.GodMode)
		{
			bool doLeftOffset = UnityEngine.Random.value >= 0.5f;
			if (doLeftOffset)
			{
				newPosition.x -= godModeOffset;
				if (newPosition.x <= Grimm.LeftEdge + 3f)
				{
					newPosition.x += godModeOffset * 2f;
				}
			}
			else
			{
				newPosition.x += godModeOffset;
				if (newPosition.x >= Grimm.RightEdge - 3f)
				{
					newPosition.x -= godModeOffset * 2f;
				}
			}
		}
		else
		{
			if (newPosition.x <= Grimm.LeftEdge + 3f)
			{
				newPosition.x = Grimm.LeftEdge + 3f;
			}
			else if (newPosition.x >= Grimm.RightEdge - 3f)
			{
				newPosition.x = Grimm.RightEdge - 3f;
			}
		}

		if (Invisible)
		{
			transform.position = newPosition;
			Grimm.FacePlayer();
			yield return Grimm.TeleportIn();
		}
		else
		{
			float time = Teleporter.TeleportEntity(gameObject, newPosition, Teleporter.TeleType.Delayed, Color.red);

			yield return new WaitForSeconds(time / 2f);
			Grimm.FacePlayer(newPosition);
			yield return new WaitForSeconds(time / 2f);
		}
	}

	public override void OnStun()
	{
		if (previousDamageAmount != 0)
		{
			damager.DamageDealt = previousDamageAmount;
			previousDamageAmount = 0;
		}
		DashSpike.enabled = false;
		Grimm.Velocity = Vector2.zero;
		groundSlash.OnStun();
		UppercutExplosion.Stop();
		//body.Sleep();
	}
}
