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
		shiftPercentage = percentage;
		hueShift = hue;
		saturationShift = sat;
		valueShift = value;
		//Debug.Log("SP = " + shiftPercentage);
		//Debug.Log("HS = " + HueShift);
		//Debug.Log("SS = " + saturationShift);
		//Debug.Log("VS = " + valueShift);
		Refresh();
	}

	void Refresh()
	{
		cameraMaterial.SetFloat("_ShiftPercentage", shiftPercentage);
		cameraMaterial.SetFloat("_HueShift", hueShift);
		cameraMaterial.SetFloat("_SatShift", saturationShift);
		cameraMaterial.SetFloat("_ValShift", valueShift);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination,cameraMaterial);
	}
}
