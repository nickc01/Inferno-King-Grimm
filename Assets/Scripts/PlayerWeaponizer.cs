using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;

public class PlayerWeaponizer : MonoBehaviour 
{
	DamageGrimm attacker;
	[SerializeField]
	GameObject SlashBox;

	[SerializeField]
	float AttackTime = 0.3f;


	void Awake()
	{
		attacker = SlashBox.GetComponent<DamageGrimm>();
	}

	bool attacking = false;
	
	// Update is called once per frame
	void Update () 
	{
		if (!attacking && Input.GetKeyDown(KeyCode.Space))
		{
			Debugger.Log("Attacking!");
			attacking = true;
			StartCoroutine(AttackRoutine());
		}
	}

	IEnumerator AttackRoutine()
	{
		var oldRotation = SlashBox.transform.eulerAngles;
		var oldPosition = SlashBox.transform.localPosition;
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			attacker.hitDirection = CardinalDirection.Up;
			//SlashBox.transform.localScale = new Vector3(oldScale.y,oldScale.x,oldScale.z);
			SlashBox.transform.eulerAngles = new Vector3(0,0,90f);
			SlashBox.transform.localPosition = new Vector3(oldPosition.y,oldPosition.x,oldPosition.z);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			attacker.hitDirection = CardinalDirection.Down;
			SlashBox.transform.eulerAngles = new Vector3(0, 0, -90f);
			SlashBox.transform.localPosition = new Vector3(oldPosition.y, -oldPosition.x, oldPosition.z);
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			attacker.hitDirection = CardinalDirection.Left;
			SlashBox.transform.eulerAngles = new Vector3(0, 0, 180f);
			SlashBox.transform.localPosition = new Vector3(-oldPosition.x, oldPosition.y, oldPosition.z);
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			attacker.hitDirection = CardinalDirection.Right;
			SlashBox.transform.eulerAngles = new Vector3(0, 0, 0f);
			SlashBox.transform.localPosition = new Vector3(oldPosition.x, oldPosition.y, oldPosition.z);
		}
		SlashBox.SetActive(true);
		yield return new WaitForSeconds(AttackTime);
		SlashBox.SetActive(false);
		SlashBox.transform.eulerAngles = oldRotation;
		SlashBox.transform.localPosition = oldPosition;
		attacking = false;
	}
}
