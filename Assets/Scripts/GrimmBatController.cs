using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrimmBatController : MonoBehaviour 
{
	public static InfernoKingGrimm CurrentGrimm { get; private set; }


	GrimmBatMovement[] grimmBats;

	void Start()
	{
		grimmBats = GetComponentsInChildren<GrimmBatMovement>(true);
	}

	public void SendOut(InfernoKingGrimm grimm)
	{
		CurrentGrimm = grimm;
		foreach (var bat in grimmBats)
		{
			bat.SendOut();
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
