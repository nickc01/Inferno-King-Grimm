using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
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
		if (position == null)
		{
			position = grimm.transform.Find("Firebat SpawnPoint").position;
		}
		return Spawn(angle, velocity, grimm.FaceDirection,position.Value);
	}


	void SetVelocity(Vector2 velocity)
	{
		rigidbody.velocity = velocity;
	}
}
