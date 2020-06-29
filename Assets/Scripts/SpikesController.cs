using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using Random = UnityEngine.Random;

public class SpikesController : MonoBehaviour 
{
	GrimmSpike[] Spikes;

	[Header("Audio")]
	[Space]
	[SerializeField]
	AudioClip SpikesPrepare;

	[SerializeField]
	AudioClip SpikesUp;

	[SerializeField]
	AudioClip SpikesDown;

	//public List<GrimmSpike> NormalSpikes;

	public IEnumerable<GrimmSpike> MiddleSpikes
	{
		get
		{
			return AllSpikes.Where((g, i) =>
			{
				float percentage = i / (float)(AllSpikes.Count - 1);
				return percentage >= (1f / 3f) && percentage <= (2f / 3f);
			});
		}
	}
	public IEnumerable<GrimmSpike> SideSpikes
	{
		get
		{
			return AllSpikes.Where((g, i) =>
			{
				float percentage = i / (float)(AllSpikes.Count - 1);
				return percentage < (1f / 3f) || percentage > (2f / 3f);
			});
		}
	}
	public List<GrimmSpike> AllSpikes;

	//public float CurrentWaitTime { get; private set; }

	class SpikeSorter : IComparer<GrimmSpike>
	{
		public int Compare(GrimmSpike x, GrimmSpike y)
		{
			return (int)x.transform.position.x - (int)y.transform.position.x;
		}
	}

	void Awake()
	{
		//Debugger.Log("Awake");
		Spikes = GetComponentsInChildren<GrimmSpike>(true);
		//GetComponentsInChildren(true, Spikes);
		AllSpikes = GetComponentsInChildren<GrimmSpike>().ToList();
		AllSpikes.Sort(new SpikeSorter());
	}

	public void Play()
	{
		StartCoroutine(PlayAsync());
	}

	IEnumerator DoSpikesAsync(IEnumerable<GrimmSpike> spikes, float prepareTime = 0.55f, float raiseTime = 0.15f, float lowerTime = 0.45f)
	{
		foreach (var spike in spikes)
		{
			spike.PrepareForAttack();
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(prepareTime);

		foreach (var spike in spikes)
		{
			spike.RaiseSpikes();
		}

		yield return new WaitForSeconds(raiseTime);

		//TODO - Shake Camera
		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(lowerTime);

		foreach (var spike in spikes)
		{
			spike.LowerSpikes();
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);
	}

	void DoSpikes(IEnumerable<GrimmSpike> spikes, float prepareTime = 0.55f, float raiseTime = 0.15f, float lowerTime = 0.45f)
	{
		StartCoroutine(DoSpikesAsync(spikes,prepareTime,raiseTime,lowerTime));
	}

	public IEnumerator PlayAsync()
	{
		//CurrentWaitTime = 1.35f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat,transform.position.y,transform.position.z);

		DoSpikes(AllSpikes.Where((g,i) => i % 2 == 0));

		yield return new WaitForSeconds(1.35f);

		/*foreach (var spike in NormalSpikes)
		{
			spike.PrepareForAttack();
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.55f);

		foreach (var spike in NormalSpikes)
		{
			spike.RaiseSpikes();
		}

		yield return new WaitForSeconds(0.15f);

		//TODO - Shake Camera
		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.45f);

		foreach (var spike in NormalSpikes)
		{
			spike.LowerSpikes();
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);*/

		//CurrentWaitTime = 0f;
	}

	public IEnumerator PlayMiddleSideAsync()
	{
		//CurrentWaitTime = 2.2f;

		bool sidesFirst = Random.value >= 0.5f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		var first = sidesFirst ? SideSpikes : MiddleSpikes;
		var second = sidesFirst ? MiddleSpikes : SideSpikes;

		DoSpikes(first,0.70f);

		yield return new WaitForSeconds(0.85f);

		yield return DoSpikesAsync(second, 0.75f);

		/*bool sidesFirst = Random.value >= 0.5f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		if (sidesFirst)
		{
			foreach (var spike in SideSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.70f);

		if (sidesFirst)
		{
			foreach (var spike in SideSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.05f);

		if (sidesFirst)
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in SideSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.40f);

		if (sidesFirst)
		{
			foreach (var spike in SideSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.35f);

		if (sidesFirst)
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in SideSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.45f);

		if (sidesFirst)
		{
			foreach (var spike in MiddleSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in SideSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);*/
		//CurrentWaitTime = 0f;
	}


	public IEnumerator PlayAlternatingAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		//var first = sidesFirst ? SideSpikes : MiddleSpikes;
		//var second = sidesFirst ? MiddleSpikes : SideSpikes;

		DoSpikes(AllSpikes.Where((g,i) => i % 2 == 0), 0.70f);

		yield return new WaitForSeconds(0.85f);

		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 1), 0.75f);
		/*var evenSpikes = AllSpikes.Where((s, i) => i % 2 == 0);
		var	oddSpikes = AllSpikes.Where((s, i) => i % 2 == 1);

		//CurrentWaitTime = 2.2f;

		bool sidesFirst = Random.value >= 0.5f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.70f);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.05f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.40f);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.35f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.45f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);
		//CurrentWaitTime = 0f;*/
	}


	public IEnumerator PlayAlternatingTripleAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		DoSpikes(AllSpikes.Where((g, i) => i % 2 == 0), 0.65f);

		yield return new WaitForSeconds(0.80f);

		DoSpikes(AllSpikes.Where((g, i) => i % 2 == 1), 0.65f);

		yield return new WaitForSeconds(0.80f);

		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 0), 0.65f);
		/*var evenSpikes = AllSpikes.Where((s, i) => i % 2 == 0);
		var oddSpikes = AllSpikes.Where((s, i) => i % 2 == 1);

		//CurrentWaitTime = 2.2f;

		bool sidesFirst = Random.value >= 0.5f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.70f);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.05f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.PrepareForAttack();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.PrepareForAttack();
			}
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.40f);

		if (sidesFirst)
		{
			foreach (var spike in evenSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in oddSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.35f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.RaiseSpikes();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.RaiseSpikes();
			}
		}

		yield return new WaitForSeconds(0.15f);

		WeaverCam.Instance.Shaker.Shake(ShakeType.AverageShake);

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.45f);

		if (sidesFirst)
		{
			foreach (var spike in oddSpikes)
			{
				spike.LowerSpikes();
			}
		}
		else
		{
			foreach (var spike in evenSpikes)
			{
				spike.LowerSpikes();
			}
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);*/
		//CurrentWaitTime = 0f;
	}

}
