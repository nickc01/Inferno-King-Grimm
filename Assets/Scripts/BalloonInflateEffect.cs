using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class BalloonInflateEffect : MonoBehaviour 
{
	[SerializeField]
	Vector3 StartScale = Vector3.one;
	[SerializeField]
	Vector3 EndScale = new Vector3(25f,25f,25f);
	[SerializeField]
	float time = 0.3f;
	[SerializeField]
	bool disableOnDone = true;

	void OnEnable()
	{
		StartCoroutine(Scaler());
		transform.localScale = Vector3.one;
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}


	IEnumerator Scaler()
	{
		transform.localScale = StartScale;
		float clock = 0f;
		do
		{
			yield return null;
			clock += Time.deltaTime;
			transform.localScale = Vector3.Lerp(StartScale,EndScale,clock / time);
		} while (clock < time);
		transform.localScale = EndScale;
		if (disableOnDone)
		{
			gameObject.SetActive(false);
		}
	}
}
