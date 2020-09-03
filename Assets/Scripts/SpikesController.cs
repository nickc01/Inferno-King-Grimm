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
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat,transform.position.y,transform.position.z);

		DoSpikes(AllSpikes.Where((g,i) => i % 2 == 0));

		yield return new WaitForSeconds(1.35f);
	}

	public IEnumerator PlayMiddleSideAsync()
	{
		bool sidesFirst = Random.value >= 0.5f;

		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		var first = sidesFirst ? SideSpikes : MiddleSpikes;
		var second = sidesFirst ? MiddleSpikes : SideSpikes;

		DoSpikes(first,0.70f);

		yield return new WaitForSeconds(0.85f);

		yield return DoSpikesAsync(second, 0.75f);
	}

	public IEnumerator PlayAlternatingAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);
		DoSpikes(AllSpikes.Where((g,i) => i % 2 == 0), 0.70f);

		yield return new WaitForSeconds(0.85f);

		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 1), 0.75f);
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
	}

	public IEnumerator PlayDashSpikes(float dashDistance, int spikesDown = int.MaxValue)
	{
		GrimmSpike nearestSpike = null;
		float distance = float.PositiveInfinity;
		int spikeIndex = 0;
		//foreach (var spike in AllSpikes)
		for (int i = 0; i < AllSpikes.Count; i++)
		{
			var spike = AllSpikes[i];
			var currentSpikeDist = Mathf.Abs(spike.transform.position.x - Player.Player1.transform.position.x);
			if (currentSpikeDist < distance)
			{
				distance = currentSpikeDist;
				nearestSpike = spike;
				spikeIndex = i;
			}
		}

		List<GrimmSpike> DownSpikes = new List<GrimmSpike>();

		float spikeX = nearestSpike.transform.position.x;

		int downSpikes = 0;
		for (int i = spikeIndex; i < AllSpikes.Count; i++)
		{
			var spike = AllSpikes[i];
			if (spike.transform.position.x > (spikeX + dashDistance))
			{
				DownSpikes.Add(spike);
				downSpikes++;
				if (downSpikes >= spikesDown)
				{
					break;
				}
			}
		}
		downSpikes = 0;
		for (int i = spikeIndex - 1; i >= 0; i--)
		{
			var spike = AllSpikes[i];
			if (spike.transform.position.x < (spikeX - dashDistance))
			{
				DownSpikes.Add(spike);
				downSpikes++;
				if (downSpikes >= spikesDown)
				{
					break;
				}
			}
		}

		DoSpikes(AllSpikes.Except(DownSpikes), 0.65f);

		yield return new WaitForSeconds(0.80f);

		yield return DoSpikesAsync(DownSpikes, 0.65f);
	}
}
