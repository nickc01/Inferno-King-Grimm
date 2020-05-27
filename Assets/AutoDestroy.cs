using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
	[SerializeField]
	float timeBeforeDestruction = 0f;

	void Start()
	{
		Destroy(gameObject, timeBeforeDestruction);
	}
}
