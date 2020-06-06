using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDisabler : MonoBehaviour 
{
	public void Disable()
	{
		gameObject.SetActive(false);
	}
}
