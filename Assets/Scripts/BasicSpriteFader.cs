using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
	public class BasicSpriteFader : MonoBehaviour
	{
		[SerializeField]
		Color startColor;
		[SerializeField]
		Color fadeOutColor = default(Color);

		[SerializeField]
		float fadeDuration = 1f;

		new SpriteRenderer renderer;

		void OnEnable()
		{
			StopAllCoroutines();
			StartCoroutine(FadeRoutine());
		}

		void OnDisable()
		{
			StopAllCoroutines();
		}

		IEnumerator FadeRoutine()
		{
			if (renderer == null)
			{
				renderer = GetComponent<SpriteRenderer>();
			}

			renderer.color = startColor;

			yield return null;

			for (float t = 0; t < fadeDuration; t += Time.deltaTime)
			{
				renderer.color = Color.Lerp(startColor, fadeOutColor, t);
				yield return null;
			}
			gameObject.SetActive(false);
		}
	}
}
