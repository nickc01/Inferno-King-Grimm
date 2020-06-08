﻿using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public abstract class GrimmMove : MonoBehaviour 
{
	private ReignitedKingGrimm grimm;
	public ReignitedKingGrimm Grimm
	{
		get
		{
			if (grimm == null)
			{
				grimm = GetComponent<ReignitedKingGrimm>();
			}
			return grimm;
		}
	}
	public Animator GrimmAnimator
	{
		get
		{
			return Grimm.GrimmAnimator;
		}
	}
	public WeaverAudioPlayer VoicePlayer
	{
		get
		{
			return Grimm.VoicePlayer;
		}
	}
	public GrimmSounds Sounds
	{
		get
		{
			return Grimm.Sounds;
		}
	}
	public GrimmDirection FaceDirection
	{
		get
		{
			return Grimm.FaceDirection;
		}
	}

	public MainPrefabs Prefabs
	{
		get
		{
			return Grimm.Prefabs;
		}
	}

	public BoxCollider2D GrimmCollider
	{
		get
		{
			return Grimm.GrimmCollider;
		}
	}

	public bool Invisible
	{
		get
		{
			return Grimm.Invisible;
		}
		set
		{
			Grimm.Invisible = value;
		}
	}

	[SerializeField]
	protected bool excludeFromRandomizer = false;


	public bool ExcludeFromRandomizer
	{
		get
		{
			return excludeFromRandomizer;
		}
		protected set
		{
			excludeFromRandomizer = value;
		}
	}

	public bool PlayerInFront()
	{
		if (Grimm.FaceDirection == GrimmDirection.Left)
		{
			return Player.Player1.transform.position.x < transform.position.x;
		}
		else if (Grimm.FaceDirection == GrimmDirection.Right)
		{
			return Player.Player1.transform.position.x > transform.position.x;
		}
		return false;
	}

	protected T GetChildObject<T>(string name) where T : Component
	{
		var child = transform.Find(name);
		if (child != null)
		{
			return child.GetComponent<T>();
		}
		else
		{
			return null;
		}
	}

	protected GameObject GetChildGameObject(string name)
	{
		var child = transform.Find(name);
		if (child != null)
		{
			return child.gameObject;
		}
		else
		{
			return null;
		}
	}

	public abstract IEnumerator DoMove();

	public virtual void OnStun()
	{
		
	}
	public virtual void OnDeath()
	{
		OnStun();
	}
}
