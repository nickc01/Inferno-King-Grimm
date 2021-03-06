using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore;

public class InfiniteGrimmDisplay : MonoBehaviour
{
	float fadeTime = 0.5f;
	float lifeTime = 5f;

	TextMeshProUGUI statsObject;

	public string Stats
	{
		get
		{
			return statsObject.text;
		}
		set
		{
			statsObject.text = value;
		}
	}

	void Awake()
	{
		statsObject = GetComponentInChildren<TextMeshProUGUI>();
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		FadeIn();
		StartCoroutine(Waiter());
		StartCoroutine(WaitForSceneTransition());
	}

	IEnumerator Waiter()
	{
		yield return new WaitForSeconds(lifeTime);
		FadeOut();
	}

	int transitions = 0;
	IEnumerator WaitForSceneTransition()
	{
		yield return new WaitUntil(() => GameManager.instance.gameState == GlobalEnums.GameState.MAIN_MENU);
		Destroy(gameObject);
	}

	Coroutine fadeCoroutine;

	public void FadeIn()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeRoutine(0f, 0f, 1f, fadeTime));
	}

	public void FadeOut()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, 0f, fadeTime));
		Destroy(gameObject, fadeTime);
	}

	IEnumerator FadeRoutine(float delay, float from, float to, float time)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		var graphics = GetComponentsInChildren<Graphic>();
		for (float i = 0; i < time; i += Time.deltaTime)
		{
			foreach (var graphic in graphics)
			{
				var oldColor = graphic.color;
				oldColor.a = Mathf.Lerp(from,to,i / time);
				graphic.color = oldColor;
			}
			yield return null;
		}
		foreach (var graphic in graphics)
		{
			var oldColor = graphic.color;
			oldColor.a = to;
			graphic.color = oldColor;
		}

		fadeCoroutine = null;
	}

	void OnDestroy()
	{
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
	}

	private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
	{
		transitions++;
	}
}
