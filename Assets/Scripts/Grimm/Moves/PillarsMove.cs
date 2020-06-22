using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class PillarsMove : GrimmMove
{
	[Header("Pillar Attack Settings")]
	[SerializeField]
	int pillarsToSpawn = 4;
	[SerializeField]
	float pillarSpawnRate = 0.75f;


	List<Pillar> SpawnedPillars = new List<Pillar>();

	public override IEnumerator DoMove()
	{
		if (Grimm.BossStage == 3)
		{
			yield return null;
			yield break;
		}

		float teleX = 0f;

		while (true)
		{
			var playerX = Player.Player1.transform.position.x;
			var selfX = transform.position.x;

			var teleAdder = Random.Range(12f, 13f);

			if (Random.value > 0.5f)
			{
				teleAdder = -teleAdder;
			}

			teleX = playerX + teleAdder;

			if (teleX >= Grimm.LeftEdge && teleX <= Grimm.RightEdge)
			{
				break;
			}
			yield return null;
		}

		if (Grimm.BossStage >= 2)
		{
			transform.position = new Vector3(teleX, 13.6f, 0f);
		}
		else
		{
			transform.position = new Vector3(teleX, 13.1f, 0f);
		}

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		GrimmAnimator.PlayAnimation("Pillar Idle");

		WeaverAudio.Play(Sounds.PillarAntic, transform.position);

		VoicePlayer.Play(Sounds.PillarCastVoice);

		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < pillarsToSpawn; i++)
		{
			Pillar pillar = null;
			if (Grimm.BossStage >= 2)
			{
				var playerPos1 = Player.Player1.transform.position;
				yield return null;
				var playerPos2 = Player.Player1.transform.position;

				var predictivePosition = (((playerPos2 - playerPos1) / Time.deltaTime) * 0.10f) + playerPos2;

				pillar = Instantiate(Prefabs.FlamePillarPrefab, predictivePosition + Prefabs.FlamePillarPrefab.transform.position, Quaternion.identity);
			}
			else
			{
				pillar = Instantiate(Prefabs.FlamePillarPrefab, Player.Player1.transform.position + Prefabs.FlamePillarPrefab.transform.position, Quaternion.identity);
			}
			SpawnedPillars.Add(pillar);

			yield return new WaitForSeconds(pillarSpawnRate);
		}
		yield return new WaitForSeconds(0.25f);
		yield return Grimm.TeleportOut();
		if (Grimm.BossStage == 2)
		{
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}

		SpawnedPillars.Clear();
	}

	public override void OnStun()
	{
		if (SpawnedPillars.Count > 0)
		{
			foreach (var pillar in SpawnedPillars)
			{
				if (pillar != null && pillar.gameObject != null)
				{
					pillar.DamagePlayer = false;
				}
			}
		}
	}
}
