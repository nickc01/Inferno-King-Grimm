using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

[ExecuteInEditMode]
public class GrimmHue : MonoBehaviour 
{
	static HashSet<GrimmHue> allGrimmHues = new HashSet<GrimmHue>();

	public static IEnumerable<GrimmHue> AllGrimmHues
	{
		get
		{
			return allGrimmHues;
		}
	}

	public float HueShiftRed
	{
		get
		{
			return hueShiftRed;
		}

		set
		{
			if (hueShiftRed != value)
			{
				hueShiftRed = value;
				UpdateHue();
			}
		}
	}

	public float HueShiftGreen
	{
		get
		{
			return hueShiftGreen;
		}

		set
		{
			if (HueShiftGreen != value)
			{
				hueShiftGreen = value;
				UpdateHue();
			}
		}
	}

	public float HueShiftBlue
	{
		get
		{
			return hueShiftBlue;
		}

		set
		{
			if (hueShiftBlue != value)
			{
				hueShiftBlue = value;
				UpdateHue();
			}
		}
	}

	[Range(0f,1f)]
	[SerializeField]
	float hueShiftRed;
	[Range(0f, 1f)]
	[SerializeField]
	float hueShiftGreen;
	[Range(0f, 1f)]
	[SerializeField]
	float hueShiftBlue;

	static float globalRedHue = 0f;
	static float globalGreenHue = 0f;
	static float globalBlueHue = 0f;

	MaterialPropertyBlock block;
	// Use this for initialization

	Renderer rend;

	void Awake()
	{
		hueShiftRed = globalRedHue;
		hueShiftGreen = globalGreenHue;
		hueShiftBlue = globalBlueHue;
		UpdateHue();
	}

	void Start()
	{
		allGrimmHues.Add(this);
	}

	void OnEnable()
	{
		allGrimmHues.Add(this);
	}

	void OnDisable()
	{
		allGrimmHues.Remove(this);
	}

	void OnDestroy()
	{
		allGrimmHues.Remove(this);
	}

	void OnValidate()
	{
		UpdateHue();
	}

	void UpdateHue()
	{
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
		}
		if (rend != null)
		{
			rend.GetPropertyBlock(block);

			block.SetColor("_HueShift", new Color(hueShiftRed,hueShiftGreen,hueShiftBlue));

			rend.SetPropertyBlock(block);
		}
	}

	public static void SetAllGrimmHues(float redHueShift,float greenHueShift,float blueHueShift)
	{
		foreach (var hue in allGrimmHues)
		{
			hue.hueShiftRed = Mathf.Clamp01(redHueShift);
			hue.hueShiftGreen = Mathf.Clamp01(greenHueShift);
			hue.hueShiftBlue = Mathf.Clamp01(blueHueShift);

			globalRedHue = Mathf.Clamp01(redHueShift);
			globalGreenHue = Mathf.Clamp01(greenHueShift);
			globalBlueHue = Mathf.Clamp01(blueHueShift);

			hue.UpdateHue();
		}
	}
}
