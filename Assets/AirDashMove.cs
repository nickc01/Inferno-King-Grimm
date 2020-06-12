using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class AirDashMove : GrimmMove
{
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

	Rigidbody2D body;
	PolygonCollider2D DashSpike;

	void Awake()
	{
		body = GetComponent<Rigidbody2D>();
		DashSpike = GetChildObject<PolygonCollider2D>("Dash Spike");
	}

	public override IEnumerator DoMove()
	{
		Debugger.Log("A");

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

			Debugger.Log("Tele X = " + teleX);

			if (teleX < 75f || teleX > 99f)
			{
				teleX = float.PositiveInfinity;
				continue;
			}

			yield return null;
		}
		while (teleX == float.PositiveInfinity);

		Debugger.Log("B");

		transform.position = new Vector3(teleX, 15f, 0f);

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		Debugger.Log("Angle To Player = " + transform.position.GetAngleBetween(Player.Player1.transform.position));

		var velocityAngle = Mathf.Clamp(transform.position.GetAngleBetween(Player.Player1.transform.position), -135f, -45f);
		var spriteAngle = velocityAngle + 90f;

		Debugger.Log("Angle = " + velocityAngle);
		//WeaverAudio.Play(Sounds.AirDashAntic, transform.position, 1, AudioChannel.Sound);

		//VoicePlayer.Play();

		yield return GrimmAnimator.PlayAnimationTillDone("Air Dash Antic");

		GrimmAnimator.PlayAnimation("Air Dash");


		body.velocity = VectorUtilities.AngleToVector(velocityAngle * Mathf.Deg2Rad, airDashSpeed);
		transform.eulerAngles = new Vector3(0f, 0f, spriteAngle);



		WeaverAudio.Play(Sounds.AirDash, transform.position);



		var effect = Instantiate(Prefabs.AirDashEffect, transform, false);
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
				Instantiate(Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (!vertical && (transform.position.x <= Grimm.LeftEdge || transform.position.x >= Grimm.RightEdge))
			{
				body.velocity = VectorUtilities.AngleToVector(-90f * Mathf.Deg2Rad, airDashSpeed);
				transform.eulerAngles = Vector3.zero;
				vertical = true;
			}
		} while (transform.position.y > Grimm.GroundY);


		body.velocity = Vector2.zero;

		transform.position = new Vector3(transform.position.x, Grimm.GroundY, transform.position.z);

		GrimmAnimator.PlayAnimation("Ground Dash Antic");

		var landPlayer = WeaverAudio.Play(Sounds.LandSound, transform.position);
		landPlayer.AudioSource.pitch = 0.9f;

		//TODO : Make Camera Shake
		WeaverCam.Instance.Shaker.Shake(ShakeType.EnemyKillShake);

		Grimm.FacePlayer();
		transform.rotation = Quaternion.identity;

		Instantiate(Prefabs.SlamEffect, transform.position + Prefabs.SlamEffect.transform.position, Quaternion.identity);

		yield return new WaitForSeconds(0.5f);

		WeaverAudio.Play(Sounds.GroundDash, transform.position);

		var groundEffect = Instantiate(Prefabs.GroundDashEffect, transform.position + Prefabs.GroundDashEffect.transform.position, Quaternion.identity);
		var dashSpeed = transform.localScale.x * groundDashSpeed;

		if (FaceDirection == GrimmDirection.Left)
		{
			groundEffect.GetComponent<SpriteRenderer>().flipX = true;
			groundEffect.transform.position -= Prefabs.GroundDashEffect.transform.position * 2f;
			dashSpeed = -dashSpeed;
		}

		body.velocity = new Vector2(dashSpeed, 0f);

		GrimmAnimator.PlayAnimation("G Dash");

		DashSpike.enabled = true;

		var dustEffect = Instantiate(Prefabs.DustGroundEffect, transform.position + Prefabs.DustGroundEffect.transform.position, Prefabs.DustGroundEffect.transform.rotation).GetComponent<ParticleSystem>();
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
				Instantiate(Prefabs.FlameTrailEffects, transform.position + flameSpawnOffset, Prefabs.FlameTrailEffects.transform.rotation);
			}
			if (horizontal && (transform.position.x <= Grimm.LeftEdge || transform.position.x >= Grimm.RightEdge))
			{
				body.velocity = Vector2.zero;//VectorTools.PolarToCartesian(-90f * Mathf.Deg2Rad, airDashSpeed);
				horizontal = false;
			}
		} while (waitTimer < groundDashTime);

		GrimmAnimator.PlayAnimation("Ground Dash Antic");

		waitTimer = 0f;

		DashSpike.enabled = false;

		dustEffect.Stop();

		do
		{
			yield return null;
			body.velocity = VectorUtilities.Decelerate(body.velocity, new Vector2(0.75f, float.NaN));
			waitTimer += Time.deltaTime;
		} while (waitTimer < 0.33f);

		yield return Grimm.TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	public override void OnStun()
	{
		DashSpike.enabled = false;
		body.velocity = Vector2.zero;
		body.Sleep();
	}
}
