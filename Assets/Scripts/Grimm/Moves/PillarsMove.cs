﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class PillarsMove : GrimmMove
{
	[Header("Pillar Attack Settings")]
	[SerializeField]
	int pillarsToSpawn = 4;
	[SerializeField]
	float pillarSpawnRate = 0.75f;

	[Header("Second Stage")]
	[SerializeField]
	int amountofPillarsEachRound = 6;
	[SerializeField]
	float timeBetweenRounds = 1.15f;
	[SerializeField]
	int amountOfRounds = 3;

	[Header("Easy Mode")]
	[SerializeField]
	float predictiveAmountStage1 = 0.1f;
	[SerializeField]
	float predictiveAmountStage2 = 0.2f;
	[SerializeField]
	float predictiveAmountStage3 = 0.2f;

	[Header("Hard Mode")]
	[SerializeField]
	float hardModePredictiveAmountStage1 = 0.1f;
	[SerializeField]
	float hardModePredictiveAmountStage2 = 0.2f;
	[SerializeField]
	float hardModePredictiveAmountStage3 = 0.2f;


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

			if (teleX >= Grimm.LeftEdge + 2f && teleX <= Grimm.RightEdge - 2f)
			{
				break;
			}
			yield return null;
		}
		Vector3 newPosition = transform.position;
		if (Grimm.BossStage >= 2)
		{
			newPosition = new Vector3(teleX, 13.6f, 0f);
		}
		else
		{
			newPosition = new Vector3(teleX, 13.1f, 0f);
		}

		Grimm.FacePlayer(newPosition);

		yield return Grimm.TeleportIn(newPosition);

		GrimmAnimator.PlayAnimation("Pillar Idle");

		WeaverAudio.PlayAtPoint(Sounds.PillarAntic, transform.position);

		VoicePlayer.Play(Sounds.PillarCastVoice);

		yield return new WaitForSeconds(0.5f / InfernoKingGrimm.InfiniteSpeed);

		for (int i = 0; i < pillarsToSpawn; i++)
		{
			Pillar pillar = null;
			//if (Grimm.BossStage >= 2)
			//{
			var playerPos1 = Player.Player1.transform.position;
			yield return null;
			var playerPos2 = Player.Player1.transform.position;

			Vector3 predictivePosition = default(Vector3);
			float predictiveAmount = 0f;

			var predictiveStage = Grimm.BossStage;
			if (Grimm.BossStage == 3 && Grimm.Settings.Infinite)
			{
				predictiveStage = 2;
			}

			switch (predictiveStage)
			{
				case 1:
					predictiveAmount = Grimm.Settings.hardMode ? hardModePredictiveAmountStage1 : predictiveAmountStage1;
					break;
				case 2:
					predictiveAmount = Grimm.Settings.hardMode ? hardModePredictiveAmountStage2 : predictiveAmountStage2;
					break;
				default:
					predictiveAmount = Grimm.Settings.hardMode ? hardModePredictiveAmountStage3 : predictiveAmountStage3;
					break;
			}
			if (Grimm.Settings.Infinite && Grimm.CurrentCycle >= 1)
			{
				predictiveAmount = 0f;
			}
			predictivePosition = (((playerPos2 - playerPos1) / (Time.deltaTime * InfernoKingGrimm.InfiniteSpeed)) * predictiveAmount) + playerPos2;
			pillar = Pillar.Create(predictivePosition + Prefabs.FlamePillarPrefab.transform.position);

			if (i == 2 && Grimm.BossStage >= 3)
			{
				newPosition = new Vector3(Grimm.RightEdge - (transform.position.x - Grimm.LeftEdge), transform.position.y, transform.position.z);

				if (Vector3.Distance(newPosition, Player.Player1.transform.position) <= 7f)
				{
					//If the player is moving right
					if (playerPos2.x > playerPos1.x)
					{
						newPosition.x = Player.Player1.transform.position.x - 6f;
					}
					else
					{
						newPosition.x = Player.Player1.transform.position.x + 6f;
					}
				}

				Teleporter.TeleportEntity(gameObject, newPosition,Teleporter.TeleType.Delayed);
			}

			SpawnedPillars.Add(pillar);

			yield return new WaitForSeconds(pillarSpawnRate / InfernoKingGrimm.InfiniteSpeed);
		}
		//}


		yield return new WaitForSeconds(0.25f / InfernoKingGrimm.GetInfiniteSpeed(1f,1.75f));
		yield return Grimm.TeleportOut();
        if (Grimm.FightingInPantheon)
        {
            yield return new WaitForSeconds(0.15f / InfernoKingGrimm.InfiniteSpeed);
        }
        else if (Grimm.BossStage >= 2 || Grimm.FightingInPantheon)
		{
			yield return new WaitForSeconds(0.3f / InfernoKingGrimm.InfiniteSpeed);
		}
		else
		{
			yield return new WaitForSeconds(0.6f / InfernoKingGrimm.InfiniteSpeed);
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
