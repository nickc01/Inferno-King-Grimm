using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffects : MonoBehaviour 
{
	public void Play()
	{
		for (int c = 0; c < transform.childCount; c++)
		{
			transform.GetChild(c).gameObject.SetActive(true);
		}
	}
}
