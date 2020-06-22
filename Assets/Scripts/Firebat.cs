﻿using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class Firebat : MonoBehaviour 
{
	public float Angle { get; private set; }
	public GrimmDirection GeneralDirection { get; private set; }
	public float DirectionAngle { get; private set; }

	float XVelocity = 0.0f;

	new Rigidbody2D rigidbody;
	new SpriteRenderer renderer;

	// Use this for initialization
	void Start () 
	{
		Destroy(gameObject, 3);
	}
	
	// Update is called once per frame
	void Update () 
	{
		rigidbody.velocity = Vector3.RotateTowards(rigidbody.velocity, Player.NearestPlayer(this).transform.position - transform.position, 30.0f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);
		//rigidbody.velocity.
	}

	public static Firebat Spawn(float angle, float velocity, GrimmDirection direction, Vector3 position)
	{
		Debugger.Log("Fire bat F");
		var fireBat = GameObject.Instantiate(MainPrefabs.Instance.FirebatPrefab, position, Quaternion.identity).GetComponent<Firebat>();

		fireBat.rigidbody = fireBat.GetComponent<Rigidbody2D>();
		fireBat.renderer = fireBat.GetComponent<SpriteRenderer>();

		//fireBat.Altitude = altitude;
		fireBat.Angle = angle;
		fireBat.XVelocity = velocity;
		fireBat.GeneralDirection = direction;

		if (direction == GrimmDirection.Right)
		{
			fireBat.SetVelocity(new Vector2(velocity, 0));
			fireBat.renderer.flipX = true;
		}
		else
		{
			fireBat.SetVelocity(new Vector2(-velocity, 0));
		}

		if (direction == GrimmDirection.Right)
		{
			fireBat.DirectionAngle = angle;
		}
		else
		{
			fireBat.DirectionAngle = 180 - angle;
		}
		fireBat.SetVelocity(fireBat.DirectionAngle.DegreesToVector() * velocity);
		Debugger.Log("Fire bat G");
		return fireBat;
	}

	/*public static Firebat Spawn(Altitude altitude, float velocity, Direction direction, Vector3 position)
	{

		float angle = 0f;
		if (altitude == Altitude.High)
		{
			angle = 
		}
		else
		{
			angle = -17f;
		}
		return Spawn(angle, velocity, direction, position);
	}*/

	public static Firebat Spawn(float angle, float velocity, ReignitedKingGrimm grimm, Vector3? position = null)
	{
		Debugger.Log("Fire bat D");
		if (position == null)
		{
			position = grimm.transform.Find("Firebat SpawnPoint").position;
		}
		Debugger.Log("Fire bat E");
		return Spawn(angle, velocity, grimm.FaceDirection,position.Value);
	}

	public static IEnumerator SendFirebatAsync(ReignitedKingGrimm grimm, float angle, float audioPitch = 1.0f, float speedMultiplier = 1f)
	{
		Debugger.Log("FIre bat C");
		var fireBatVelocity = grimm.transform.localScale.x * 20f * speedMultiplier;

		Spawn(angle, fireBatVelocity, grimm);
		FirebatFirePillar.Spawn(grimm);

		var fireAudio = WeaverAudio.Play(grimm.Sounds.GrimmBatFire, grimm.transform.position, 1.0f, AudioChannel.Sound);
		fireAudio.AudioSource.pitch = audioPitch;

		yield return new WaitForSeconds(0.3f);

		yield break;
	}

	public static void SendFirebat(ReignitedKingGrimm grimm, float angle, float pitch = 1.0f, float speedMultiplier = 1f)
	{
		Debugger.Log("FIre bat B");
		CoroutineUtilities.StartCoroutine(SendFirebatAsync(grimm, angle, pitch, speedMultiplier));
	}


	void SetVelocity(Vector2 velocity)
	{
		rigidbody.velocity = velocity;
	}
}
