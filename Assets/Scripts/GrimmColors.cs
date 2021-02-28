using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*[CreateAssetMenu(menuName = "IKG/GrimmColors")]
public class GrimmColors : ScriptableObject
{
	public static GrimmColors Instance { get; private set; }

	[SerializeField]
	List<Material> GrimmMaterials;

	[Range(0f, 1f)]
	[SerializeField]
	float hueShiftRed;
	[Range(0f, 1f)]
	[SerializeField]
	float hueShiftGreen;
	[Range(0f, 1f)]
	[SerializeField]
	float hueShiftBlue;

	void Awake()
	{
		Instance = this;
	}

	void OnValidate()
	{
		Refresh();
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
				Refresh();
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
				Refresh();
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
				Refresh();
			}
		}
	}

	public void SetHues(float red, float green, float blue)
	{
		hueShiftRed = red;
		hueShiftGreen = green;
		hueShiftBlue = blue;
		Refresh();
	}

	public void AddMaterial(Material material)
	{
		GrimmMaterials.Add(material);
		Refresh();
	}
	public void RemoveMaterial(Material material)
	{
		GrimmMaterials.Remove(material);
		Refresh();
	}

	public void Refresh()
	{
		foreach (var material in GrimmMaterials)
		{
			material.SetColor("_HueShift", new Color(hueShiftRed, hueShiftGreen, hueShiftBlue));
		}
	}
}

*/