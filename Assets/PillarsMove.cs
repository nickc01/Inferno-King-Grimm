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

		transform.position = new Vector3(teleX, 13.1f, 0f);

		Grimm.FacePlayer();

		yield return Grimm.TeleportIn();

		GrimmAnimator.PlayAnimation("Pillar Idle");

		WeaverAudio.Play(Sounds.PillarAntic, transform.position);

		VoicePlayer.Play(Sounds.PillarCastVoice);

		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < pillarsToSpawn; i++)
		{
			var pillar = Instantiate(Prefabs.FlamePillarPrefab, Player.Player1.transform.position + Prefabs.FlamePillarPrefab.transform.position, Quaternion.identity);
			SpawnedPillars.Add(pillar);

			yield return new WaitForSeconds(0.75f);
		}
		yield return new WaitForSeconds(0.25f);
		yield return Grimm.TeleportOut();
		yield return new WaitForSeconds(0.6f);

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
