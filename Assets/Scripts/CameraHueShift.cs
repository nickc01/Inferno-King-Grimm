using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class CameraHueShift : MonoBehaviour 
{
	static CameraHueShift _currentHueShifter;
	public static CameraHueShift CurrentHueShifter
    {
		get
        {
			if (_currentHueShifter == null)
			{
				var prefab = WeaverAssets.LoadAssetFromBundle<GameObject, IKG.InfernoGrimmMod>("Camera Hue Shifter");
				AddToCamera(prefab.GetComponent<CameraHueShift>());
			}
			return _currentHueShifter;
		}
    }

	public static bool HueShifterCreated => _currentHueShifter != null;

	public static void Remove()
	{
		if (_currentHueShifter != null)
		{
			Destroy(_currentHueShifter);
			_currentHueShifter = null;
		}
	}

	public enum Mode
    {
		Off,
		Performance,
		Quality
    }

	[SerializeField]
	Mode colorMode = Mode.Performance;

	public Mode ColorMode
    {
		get => colorMode;
        set
        {
			colorMode = value;
			Refresh();
        }
    }

	//public Material cameraMaterial;

	public Material performanceMaterial;
	public Material qualityMaterial;

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

	public static CameraHueShift AddToCamera(CameraHueShift prefab)
    {
		if (_currentHueShifter == null)
        {
			_currentHueShifter = WeaverCamera.Instance.gameObject.AddComponent<CameraHueShift>();
			_currentHueShifter.performanceMaterial = prefab.performanceMaterial;
			_currentHueShifter.qualityMaterial = prefab.qualityMaterial;
			_currentHueShifter.shiftPercentage = prefab.shiftPercentage;

			_currentHueShifter.hueShift = prefab.hueShift;
			_currentHueShifter.saturationShift = prefab.saturationShift;
			_currentHueShifter.valueShift = prefab.valueShift;

			_currentHueShifter.colorMode = prefab.colorMode;

			_currentHueShifter.Refresh();
		}
		return CurrentHueShifter;
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

	static int _satID;
    static int _brightnessID;
    static int _shiftPercentageID;
    static int _hueShiftID;

	static List<Material> coloredMaterials;
	public List<Material> ColoredMaterials
	{
		get
		{
			return coloredMaterials;
        }
		set
		{
            _satID = Shader.PropertyToID("_Sat");
            _brightnessID = Shader.PropertyToID("_Brightness");
            _shiftPercentageID = Shader.PropertyToID("_ShiftPercentage");
			_hueShiftID = Shader.PropertyToID("_HueShift");

			if (coloredMaterials == null)
			{
                coloredMaterials = value.ToList();
            }
			else
			{
                foreach (var mat in value)
                {
                    if (!coloredMaterials.Contains(mat))
                    {
						coloredMaterials.Add(mat);
                    }
                }
            }

			/*foreach (var mat in coloredMaterials)
			{
				mat.SetFloat(_satID, 1.4f);
				mat.SetFloat(_brightnessID, 0.23f);
            }*/

			Refresh();
        }
	}

	public void Refresh()
	{
		if (coloredMaterials != null)
		{
			foreach (var mat in coloredMaterials)
			{
                //mat.SetFloat(_shiftPercentageID, shiftPercentage);
                mat.SetFloat(_shiftPercentageID, shiftPercentage);
                mat.SetFloat(_hueShiftID, hueShift);
            }
		}
        /*if (colorMode == Mode.Quality)
        {
			qualityMaterial.SetFloat("_ShiftPercentage", shiftPercentage);
			qualityMaterial.SetFloat("_HueShift", hueShift);
		}
        else if (colorMode == Mode.Performance)
        {
			performanceMaterial.SetFloat("_ShiftPercentage", shiftPercentage);
		}*/

		//cameraMaterial.SetFloat("_SatShift", saturationShift);
		//cameraMaterial.SetFloat("_ValShift", valueShift);
	}

	/*void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        switch (colorMode)
        {
            case Mode.Performance:
				Graphics.Blit(source, destination, performanceMaterial);
				break;
            case Mode.Quality:
				Graphics.Blit(source, destination, qualityMaterial);
				break;
            default:
                break;
        }
	}*/
}
