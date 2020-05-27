using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
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

	void Awake()
	{
		Debugger.Log("Awake");
		Spikes = GetComponentsInChildren<GrimmSpike>(true);
		//GetComponentsInChildren(true, Spikes);
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play()
	{
		StartCoroutine(PlayAsync());
	}

	public IEnumerator PlayAsync()
	{
		float positionFloat = Random.Range(66f, 67.125f);

		transform.position = new Vector3(positionFloat,transform.position.y,transform.position.z);

		foreach (var spike in Spikes)
		{
			spike.PrepareForAttack();
		}

		WeaverAudio.Play(SpikesPrepare, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.55f);

		foreach (var spike in Spikes)
		{
			spike.RaiseSpikes();
		}

		yield return new WaitForSeconds(0.15f);

		//TODO - Shake Camera

		WeaverAudio.Play(SpikesUp, Player.Player1.transform.position);

		yield return new WaitForSeconds(0.45f);

		foreach (var spike in Spikes)
		{
			spike.LowerSpikes();
		}

		WeaverAudio.Play(SpikesDown, Player.Player1.transform.position);
	}

}
