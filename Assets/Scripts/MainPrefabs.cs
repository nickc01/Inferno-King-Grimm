using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefabs", menuName = "Main Prefabs")]
public class MainPrefabs : ScriptableObject
{
	public static MainPrefabs Instance;

	public GameObject FirebatPrefab;
	public GameObject FirebatPillarPrefab;
	public GameObject AirDashEffect;

	public GameObject FlameTrailEffects;
	public GameObject SlamEffect;
	public GameObject GroundDashEffect;
	public GameObject DustGroundEffect;
	public SpikesController spikeControllerPrefab;
	public Pillar FlamePillarPrefab;
	public Rigidbody2D UppercutFireball;
	public GrimmBall GrimmBall;
	public GameObject StunEffect;
	public GrimmBatController BatControllerPrefab;
}

