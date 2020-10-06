using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPoundAfterburn : MonoBehaviour 
{
	[SerializeField]
	float lifetime = 3f;

	[SerializeField]
	float colliderTime = 1f;

	new Collider2D collider;
	ParticleSystem particles;
	void Start () 
	{
		particles = GetComponent<ParticleSystem>();
		collider = GetComponent<Collider2D>();
		particles.Play();
		StartCoroutine(Timer(colliderTime, () =>
		{
			if (collider != null)
			{
				collider.enabled = false;
			}
		}));

		StartCoroutine(Timer(lifetime, () =>
		{
			Destroy(gameObject);
		}));


	}

	IEnumerator Timer(float time, Action onDone)
	{
		yield return new WaitForSeconds(time);
		onDone();
	}
}
