using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefabs", menuName = "Main Prefabs")]
public class MainPrefabs : ScriptableObject
{
	public static MainPrefabs Instance;

	public Firebat FirebatPrefab;
	public FirebatFirePillar FirebatPillarPrefab;
	public GameObject AirDashEffect;

	public GameObject FlameTrailEffects;
	public GameObject SlamEffect;
	public GameObject GroundDashEffect;
	public ParticleSystem DustGroundEffect;
	public SpikesController spikeControllerPrefab;
	public Pillar FlamePillarPrefab;
	public UppercutFireball UppercutFireball;
	public GrimmBall GrimmBall;
	public GameObject StunEffect;
	public GrimmBatController BatControllerPrefab;
	public HomingBall HomingBallPrefab;
	public GrimmGlow GlowPrefab;
	public GameObject GroundPoundAfterburn;
	public GameObject RedBurst;
}

