using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GrimmMoveSet : MonoBehaviour 
{
	public ReignitedKingGrimm Grimm { get; private set; }

	[SerializeField]
	protected bool excludeFromRandomizer = false;

	void Awake()
	{
		Grimm = GetComponent<ReignitedKingGrimm>();
	}


	public bool ExcludeFromRandomizer
	{
		get
		{
			return excludeFromRandomizer;
		}
	}

	public abstract void DoMove();
}
