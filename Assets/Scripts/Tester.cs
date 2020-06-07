using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class Tester : MonoBehaviour
{

	[SerializeField]
	bool runInStateOnly = true;

	[SerializeField]
	RunningState stateToRunIn = RunningState.Editor;


	HealthManager manager;
	GameObject testAttacker;
	// Use this for initialization
	void Start()
	{
		StartCoroutine(TestRunner());
		/*if (runInStateOnly == false || stateToRunIn == ImplFinder.State)
		{
			manager = GetComponent<HealthManager>();
			//testAttacker = new GameObject("Tester");
			StartCoroutine(TestRunner());
		}*/
	}

	IEnumerator TestRunner()
	{

		while (true)
		{
			Debugger.Log("Test Run!");
			yield return new WaitForSeconds(2f);

			try
			{
				var position = transform.position;
				Debugger.Log("Teleporting!");
				Teleporter.TeleportEntity(gameObject, new Vector3(position.x - 10f, position.y, position.z), Teleporter.TeleType.Delayed, Color.red);
			}
			catch (Exception e)
			{
				Debugger.Log("Teleportation Exception = " + e);
			}
			/*manager.Hit(new HitInfo()
			{
				Attacker = testAttacker,
				AttackStrength = 1f,
				AttackType = AttackType.Nail,
				Damage = 1,
				Direction = 0,
				IgnoreInvincible = false
			});*/
		}

	}

	// Update is called once per frame
	void Update()
	{

	}
}
