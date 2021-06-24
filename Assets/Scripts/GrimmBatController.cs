using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrimmBatController : MonoBehaviour 
{
	//public static InfernoKingGrimm CurrentGrimm { get; private set; }


	GrimmBatMovement[] grimmBats;

	GrimmRealBat _realBat;
	public GrimmRealBat RealBat
	{
		get
		{
			if (_realBat == null)
			{
				_realBat = GetComponentInChildren<GrimmRealBat>(true);
			}
			return _realBat;
		}
	}

	void Start()
	{
		grimmBats = GetComponentsInChildren<GrimmBatMovement>(true);
	}

	public void SendOut(InfernoKingGrimm grimm)
	{
		RealBat.Grimm = grimm;
		//CurrentGrimm = grimm;
		foreach (var bat in grimmBats)
		{
			bat.SendOut(grimm);
		}
	}


	public void BringIn()
	{
		foreach (var bat in grimmBats)
		{
			bat.BringIn();
		}
	}
}
