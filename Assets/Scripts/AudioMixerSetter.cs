using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;

[RequireComponent(typeof(AudioSource))]
public class AudioMixerSetter : MonoBehaviour
{
	enum MixerChannel
	{
		None,
		Master,
		Music,
		SFX
	}

	[SerializeField]
	MixerChannel ChannelToUse = MixerChannel.None;

	void Awake()
	{
		var audioSource = GetComponent<AudioSource>();
		if (audioSource != null)
		{
			switch (ChannelToUse)
			{
				case MixerChannel.Master:
					audioSource.outputAudioMixerGroup = WeaverAudio.MasterMixerGroup;
					break;
				case MixerChannel.Music:
					audioSource.outputAudioMixerGroup = WeaverAudio.MusicMixerGroup;
					break;
				case MixerChannel.SFX:
					audioSource.outputAudioMixerGroup = WeaverAudio.SoundsMixerGroup;
					break;
			}
		}
	}
}

