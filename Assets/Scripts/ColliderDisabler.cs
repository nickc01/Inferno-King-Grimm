using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDisabler : MonoBehaviour 
{
	float time = 1f;

	[SerializeField]
	public float clock = 0f;

	void Update () 
	{
		clock += Time.deltaTime;
		if (clock >= time)
		{
			GetComponent<Collider2D>().enabled = false;
		}
	}
}
