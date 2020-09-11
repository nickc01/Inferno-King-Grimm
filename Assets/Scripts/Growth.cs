using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




public class Growth : MonoBehaviour
{
	public float lifeTime = 0f;


	public bool DisableOnCollision = true;
	public float GrowthSpeed = 1.5f;


	void Update()
	{
		lifeTime += Time.deltaTime;

		transform.localScale += new Vector3(GrowthSpeed * Time.deltaTime,GrowthSpeed * Time.deltaTime,0f);
	}


	void OnTriggerEnter2D(Collider2D collision)
	{
		if (DisableOnCollision && lifeTime >= 0.3f && collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			GrowthSpeed = 0f;
		}
	}
}

