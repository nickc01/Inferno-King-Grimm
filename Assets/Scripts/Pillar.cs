using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;

public class Pillar : MonoBehaviour
{

	//[SerializeField]
	//AudioClip FlameChargeSound;

	[SerializeField]
	float PillarSpawnY = 5.1f;



	[SerializeField]
	AudioClip FlameExplode;

	DamageHero damager;

	public bool DamagePlayer
	{
		get
		{
			return damager.enabled;
		}
		set
		{
			damager.enabled = value;
		}
	}

	// Use this for initialization
	void Start () 
	{
		damager = GetComponentInChildren<DamageHero>(true);
		StartCoroutine(PillarRoutine());
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	IEnumerator PillarRoutine()
	{
		transform.position = new Vector3(transform.position.x ,PillarSpawnY,transform.position.z);
		//WeaverAudio.Play(FlameChargeSound, transform.position);

		yield return new WaitForSeconds(0.5f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.EnemyKillShake);

		WeaverAudio.Play(FlameExplode, transform.position);

		yield return new WaitForSeconds(0.3f);

		var afterBurnObject = transform.Find("Pt Afterburn").gameObject;

		var afterBurn = afterBurnObject.GetComponent<ParticleSystem>();
		afterBurn.Play();

		var collider = afterBurnObject.GetComponent<CircleCollider2D>();

		collider.enabled = true;

		yield return new WaitForSeconds(1f);

		collider.enabled = false;

		yield return new WaitForSeconds(0.22f);

		Destroy(gameObject);

		//yield break;
	}
}
