using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace Assets.Scripts
{
	public class IKGSettings : Panel
	{
		//[SerializeField]
		//[SettingField(EnabledType.Never)]
		//Material cameraMaterial;

		[Tooltip(@"Checking this will make the boss fight considerably harder

For those who want a bigger challenge")]
		[SettingField(EnabledType.MenuOnly)]
		public bool hardMode = false;

		[Tooltip(@"Checking this will allow you to customize the health to be whatever you want.

The value can be set below")]
		[SerializeField]
		[SettingField(EnabledType.Never)]
		bool enableCustomHealth = false;

		[SettingField(EnabledType.MenuOnly)]
		public bool EnableCustomHealth
		{
			get
			{
				return enableCustomHealth;
			}
			set
			{
				if (enableCustomHealth != value)
				{
					enableCustomHealth = value;
					if (!InPauseMenu)
					{
						var healthElement = GetElement("CustomHealthValue");
						healthElement.Visible = enableCustomHealth;
						if (enableCustomHealth)
						{
							healthElement.Order = GetElement("EnableCustomHealth").Order + 1;
						}
					}
				}
			}
		}

		[Tooltip(@"The custom health the boss will use.

NOTE: Setting this value too low may cause issues, so do so with caution

For reference, here are all the normal health values the boss uses:

Normal Mode Attuned		: 1300
Normal Mode Ascended	: 1450
Normal Mode Radiant		: 1600

Hard Mode Attunded		: 1950
Hard Mode Ascended		: 2000
Hard Mode Radiant		: 2050")]
		public int CustomHealthValue = 1300;

		[Tooltip(@"Checking this will disable color effects that take place throughout the fight")]
		public bool DisableColorEffects = false;

		[Tooltip(@"This determines how difficult the pufferfish attack of the fight will be

-Default:  Leaves everything set to their default values

-Easy:     A toned down version of the attack

-Medium:   The same difficulty as regular IKG

-Hard:     The same difficutly as Absolute IKG

-Off:      Turns the attack off completely
")]
		public PufferFishDifficulty PufferFishDifficulty;

		[SerializeField]
		[SettingField(EnabledType.Never)]
		bool blueMode = false;

		[SettingField(EnabledType.Both)]
		[SettingDescription("Yo, listen up here's a story</br>About a little guy</br>" +
			"That lives in a blue world</br>And all day and all night</br>And everything he sees is just blue</br>" +
			"Like him inside and outside</br></br:15>(All red colors become blue colors)")]
		public bool BlueMode
		{
			get
			{
				return blueMode;
			}
			set
			{
				if (blueMode != value)
				{
					blueMode = value;
					UpdateBlueState();
				}
			}
		}

		[Header("God Mode")]
		[Tooltip("If set to true, Inferno God Grimm will have 1.75x health than Inferno King Grimm")]
		public bool IncreasedGodModeHealth = true;

		public override string TabName
		{
			get
			{
				return "Inferno King Grimm";
			}
		}

		protected override void OnPanelOpen()
		{
			var healthElement = GetElement("CustomHealthValue");
			if (!InPauseMenu)
			{
				healthElement.Visible = enableCustomHealth;
				healthElement.Order = GetElement("EnableCustomHealth").Order + 1;
			}
			else
			{
				healthElement.Visible = false;
			}
		}

		protected override void OnRegister()
		{
			UpdateBlueState();
		}

		[AfterCameraLoad]
		static void CameraInit()
		{
			UpdateBlueState();
		}

		static Material cameraMaterial;

		static void UpdateBlueState()
		{
			if (cameraMaterial == null)
			{
				cameraMaterial = WeaverAssets.LoadAssetFromBundle<Material>("infernogrimmmod", "CameraMaterial");
				//WeaverLog.Log("Camera Material = " + cameraMaterial);
				//WeaverLog.Log("Camera Material = " + cameraMaterial);
				if (cameraMaterial == null)
				{
					return;
				}
			}
			var settings = GetSettings<IKGSettings>();
			if (settings == null)
			{
				return;
			}
			if (WeaverCamera.Instance == null)
			{
				return;
			}
			var cameraHueShift = WeaverCamera.Instance.GetComponent<CameraHueShift>();
			if (cameraHueShift == null)
			{
				cameraHueShift = WeaverCamera.Instance.gameObject.AddComponent<CameraHueShift>();
				cameraHueShift.cameraMaterial = cameraMaterial;
			}
			if (settings.BlueMode)
			{
				cameraHueShift.SetValues(1f, 0.1f, 0.1f, 0.1f);
			}
			else
			{
				cameraHueShift.SetValues(0f, 0f, 0f, 0f);
			}
			cameraHueShift.Refresh();
		}

	}
}
