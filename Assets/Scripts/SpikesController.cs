using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Settings;
using WeaverCore.Utilities;
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

	[SerializeField]
	float spikeSpeedCap = 4f;
	//[SerializeField]
	//float prepareSpikeSpeedCap = 3f;

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

	void Start()
	{
		//Debugger.Log("Awake");
		Spikes = GetComponentsInChildren<GrimmSpike>(true);
		//GetComponentsInChildren(true, Spikes);
		AllSpikes = GetComponentsInChildren<GrimmSpike>().ToList();
		if (!GlobalSettings.GetSettings<IKGSettings>().Infinite)
		{
			AllSpikes.RemoveAll(s => s.name.Contains("Infinite"));
		}
		AllSpikes.Sort(new SpikeSorter());
	}

	public void Play()
	{
		StartCoroutine(PlayAsync());
	}

	IEnumerator DoSpikesAsync(IEnumerable<GrimmSpike> spikes, float prepareTime = 0.55f, float raiseTime = 0.15f, float lowerTime = 0.45f, float positionalRandomness = 0f)
	{
		//List<float> originalLocalXValues = null;
		//if (positionalRandomness > 0f)
		//{
			//originalLocalXValues = new List<float>();
			/*foreach (var spike in spikes)
			{
				//originalLocalXValues.Add(spike.transform.localPosition.x);
				spike.transform.SetXLocalPosition(spike.transform.localPosition.x + Random.Range(-positionalRandomness,positionalRandomness));
			}*/
		//}

		//int index = 0;
		foreach (var spike in spikes)
		{
			spike.PrepareForAttack(spike.OriginalPosition + new Vector3(Random.Range(-positionalRandomness, positionalRandomness),0f)/*new Vector3(originalLocalXValues[index],spike.transform.GetYPosition(),spike.transform.GetZPosition())*/, prepareTime);
			//index++;
		}

		WeaverAudio.PlayAtPoint(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(prepareTime);

		foreach (var spike in spikes)
		{
			spike.RaiseSpikes(raiseTime);
		}

		yield return new WaitForSeconds(raiseTime);

		//TODO - Shake Camera
		CameraShaker.Instance.Shake(ShakeType.AverageShake);

		WeaverAudio.PlayAtPoint(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(lowerTime);

		//int index = 0;
		foreach (var spike in spikes)
		{
			if (positionalRandomness > 0f)
			{
				spike.LowerSpikes(false,0f/*originalLocalXValues[index]*/);
			}
			else
			{
				spike.LowerSpikes(false, 0f);
			}
			//index++;
		}

		WeaverAudio.PlayAtPoint(SpikesDown, Player.Player1.transform.position);
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

	public IEnumerator DoRegularAsync(float prepareTime = 0.70f)
	{
		float positionFloat = Random.Range(65f, 68.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);
		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 0), prepareTime / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap), positionalRandomness: 1.1f, lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));
	}

	public IEnumerator PlayAlternatingAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);
		DoSpikes(AllSpikes.Where((g,i) => i % 2 == 0), 0.70f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap), lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));

		yield return new WaitForSeconds(((0.85f - 0.15f) / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap)) + 0.15f);

		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 1), 0.75f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap), lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));
	}


	public IEnumerator PlayAlternatingTripleAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat, transform.position.y, transform.position.z);

		DoSpikes(AllSpikes.Where((g, i) => i % 2 == 0), 0.65f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap), lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));

		yield return new WaitForSeconds(((0.80f - 0.15f) / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap)) + 0.15f);

		DoSpikes(AllSpikes.Where((g, i) => i % 2 == 1), 0.65f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap),lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));

		yield return new WaitForSeconds(((0.80f - 0.15f) / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap)) + 0.15f);

		yield return DoSpikesAsync(AllSpikes.Where((g, i) => i % 2 == 0), 0.65f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap),lowerTime: 0.45f / InfernoKingGrimm.GetInfiniteSpeed(1f,spikeSpeedCap));
	}

	public IEnumerator PlayDashSpikes(float dashDistance, int spikesDown = int.MaxValue)
	{
		var speed = Mathf.Clamp(InfernoKingGrimm.GetInfiniteSpeed(0.5f, spikeSpeedCap), 1f, 1.5f);
		dashDistance /= speed;
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

		DoSpikes(AllSpikes.Except(DownSpikes), 0.65f / speed,lowerTime: 0.45f / speed);

		yield return new WaitForSeconds(((0.80f - 0.15f) / speed) + 0.15f);

		yield return DoSpikesAsync(DownSpikes, 0.65f / speed, lowerTime: 0.45f / speed);
	}
}
