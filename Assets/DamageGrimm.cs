using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Helpers;
using WeaverCore.Interfaces;
using WeaverCore.Helpers;

public class DamageGrimm : MonoBehaviour 
{
	public int damage = 2;
	public AttackType attackType;
	[HideInInspector]
	public CardinalDirection hitDirection;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		Debugger.Log("2 Collided With " + collision.transform.gameObject.name);
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		Debugger.Log("Collided With " + collider.transform.gameObject.name);
		IHittable hittable = null;
		if ((hittable = collider.GetComponent<IHittable>()) != null)
		{
			hittable.Hit(new HitInfo()
			{
				Attacker = gameObject,
				Damage = damage,
				AttackStrength = 1f,
				AttackType = attackType,
				Direction = hitDirection.ToDegrees(),
				IgnoreInvincible = false
			});
		}
	}
}
