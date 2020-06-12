using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;
using Random = UnityEngine.Random;

public class BalloonMove : GrimmMove
{


	GameObject BalloonFireballShoot;
	GameObject BalloonCollider;
	GrimmHealthManager healthManager;
	ParticleSystem BalloonParticles;

	void Awake()
	{
		healthManager = GetComponent<GrimmHealthManager>();

		BalloonParticles = GetChildObject<ParticleSystem>("Balloon Particles");
		BalloonFireballShoot = GetChildGameObject("Balloon Fireball Loop Audio");
		BalloonCollider = GetChildGameObject("Balloon Collider");
	}

	public override IEnumerator DoMove()
	{
		transform.position = transform.position.With(x: 85.77f, y: 13.5f);

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

		BalloonFireballShoot.SetActive(true);

		WeaverAudio.Play(Sounds.GrimmScream, transform.position);
		WeaverAudio.Play(Sounds.InflateSoundEffect, transform.position);

		//TODO Broadcast CROWD CLAP DELAY EVENT

		WeaverEvents.BroadcastEvent("CROWD CLAP DELAY");

		var angle = Random.Range(0f, 360f);

		GrimmAnimator.PlayAnimation("Balloon");

		Grimm.FacePlayer();

		BalloonCollider.SetActive(true);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);
		WeaverCam.Instance.Shaker.SetRumble(RumbleType.RumblingSmall);

		BalloonParticles.Play();

		healthManager.Invincible = true;

		int amountOfWaves = 12;

		int ballCounter = 0;
		int waveCounter = 0;

		var ballSpawn = transform.position.With(z: 0.001f);

		for (int i = 0; i < amountOfWaves; i++)
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
		}

		WeaverCam.Instance.Shaker.SetRumble(RumbleType.None);

		yield return new WaitForSeconds(0.75f);

		BalloonParticles.Stop();

		healthManager.Invincible = false;

		BalloonCollider.SetActive(false);

		//TODO - Broadcast CROWD IDLE EVENT

		WeaverEvents.BroadcastEvent("CROWD IDLE");

		BalloonFireballShoot.SetActive(false);

		WeaverAudio.Play(Sounds.DeflateSoundEffect, transform.position);

		yield return Grimm.TeleportOut();

		yield return new WaitForSeconds(0.6f);
	}

	public override void OnStun()
	{
		WeaverCam.Instance.Shaker.SetRumble(RumbleType.None);
		BalloonFireballShoot.SetActive(false);
		BalloonParticles.Stop();
	}
}

