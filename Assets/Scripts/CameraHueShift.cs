using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class CameraHueShift : MonoBehaviour 
{
	public Material cameraMaterial;

	[SerializeField]
	float shiftPercentage = 0f;

	public float ShiftPercentage
	{
		get
		{
			return shiftPercentage;
		}
		set
		{
			if (shiftPercentage != value)
			{
				shiftPercentage = value;
				Refresh();
			}
		}
	}

	[SerializeField]
	float hueShift = 0f;

	public float HueShift
	{
		get
		{
			return hueShift;
		}
		set
		{
			if (hueShift != value)
			{
				hueShift = value;
				Refresh();
			}
		}
	}

	[SerializeField]
	float saturationShift = 0f;

	public float SaturationShift
	{
		get
		{
			return saturationShift;
		}
		set
		{
			if (saturationShift != value)
			{
				saturationShift = value;
				Refresh();
			}
		}
	}

	[SerializeField]
	float valueShift = 0f;

	public float ValueShift
	{
		get
		{
			return valueShift;
		}
		set
		{
			if (valueShift != value)
			{
				valueShift = value;
				Refresh();
			}
		}
	}

	public void SetValues(float percentage, float hue, float sat, float value)
	{
		bool changed = false;
		if (shiftPercentage != percentage)
		{
			shiftPercentage = percentage;
			changed = true;
		}
		if (hueShift != hue)
		{
			hueShift = hue;
			changed = true;
		}
		if (saturationShift != sat)
		{
			saturationShift = sat;
			changed = true;
		}
		if (valueShift != value)
		{
			valueShift = value;
			changed = true;
		}
		//Debug.Log("SP = " + shiftPercentage);
		//Debug.Log("HS = " + HueShift);
		//Debug.Log("SS = " + saturationShift);
		//Debug.Log("VS = " + valueShift);
		if (changed)
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		cameraMaterial.SetFloat("_ShiftPercentage", shiftPercentage);
		cameraMaterial.SetFloat("_HueShift", hueShift);
		//cameraMaterial.SetFloat("_SatShift", saturationShift);
		//cameraMaterial.SetFloat("_ValShift", valueShift);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination,cameraMaterial);
	}
}
