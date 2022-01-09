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
	public class IKGSettings : GlobalSettings
	{
		[Tooltip("Checking this will make the boss fight considerably harder</br:2>For those who want a bigger challenge")]
		[SettingField(EnabledType.AlwaysVisible)]
		[SettingOrder(0)]
		public bool hardMode = false;

		[Tooltip("Checking this will allow you to customize the health to be whatever you want.</br:2>The value can be set below")]
		[SerializeField]
		[SettingField(EnabledType.Hidden)]
		bool enableCustomHealth = false;

		[SettingField(EnabledType.AlwaysVisible)]
		[SettingOrder(1)]
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
					//if (!InFight)
					//{
						var healthElement = GetElement("CustomHealthValue");
						healthElement.Visible = !InFight && enableCustomHealth;
						if (enableCustomHealth)
						{
							healthElement.Order = GetElement("EnableCustomHealth").Order + 1;
						}
					//}
				}
			}
		}

		[Tooltip("The custom health the boss will use.</br:2>NOTE: Setting this value too low may cause issues, so do so with caution</br:2>" +
			"For reference, here are all the normal health values the boss uses:</br:2>Normal Mode Attuned</sp:6>: 1300</br>Normal Mode Ascended</sp:5>: 1450</br>" +
			"Normal Mode Radiant</sp:6>: 1600</br:2>Hard Mode Attunded</sp:5>: 1950</br>Hard Mode Ascended</sp:5>: 2000</br>Hard Mode Radiant</sp:5>: 2050")]
		[SettingOrder(2)]
		public int CustomHealthValue = 1300;

		[Tooltip("Checking this will disable color effects that take place throughout the fight")]
		[SettingOrder(3)]
		public bool DisableColorEffects = false;

		[Tooltip("This determines how difficult the pufferfish attack of the fight will be</br:2>-Default:  Leaves everything set to their default values</br>-Easy:     " +
			"A toned down version of the attack</br>-Medium:   The same difficulty as regular IKG</br>-Hard:     The same difficutly as Absolute IKG</br>-Off:      " +
			"Turns the attack off completely")]
		[SettingOrder(4)]
		public PufferFishDifficulty PufferFishDifficulty;

		[SerializeField]
		[SettingField(EnabledType.Hidden)]
		bool _infinite = false;
		[SettingField(EnabledType.AlwaysVisible)]
		[SettingDescription("If set to true, the boss will be infinite, and will get harder the longer you play.</br:2> When you die, your score will be displayed, and stored to a file in the Mods Directory")]
		[SettingOrder(5)]
		public bool Infinite
		{
			get => _infinite;
			set
			{
				_infinite = value;
				UpdateInfiniteState();
			}
		}

		[SettingOrder(6)]
		[SettingField(EnabledType.AlwaysVisible)]
		[Tooltip("Improves performance of the fight by removing decoration objects")]
		public bool PerformanceMode = false;

		[SerializeField]
		[SettingField(EnabledType.Hidden)]
		bool blueMode = false;

		[SettingField(EnabledType.AlwaysVisible)]
		[SettingDescription("Yo, listen up here's a story</br>About a little guy</br>" +
			"That lives in a blue world</br>And all day and all night</br>And everything he sees is just blue</br>" +
			"Like him inside and outside</br></br:15>(All red colors become blue colors)")]
		[SettingOrder(7)]
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
		[SettingField(EnabledType.AlwaysVisible)]
		[SettingOrder(8)]
		public bool IncreasedGodModeHealth = true;

		public override string TabName
		{
			get
			{
				return "Inferno King Grimm";
			}
		}

		static bool InFight => InfernoKingGrimm.GrimmsFighting.Count > 0;

		protected override void OnPanelOpen()
		{
			UpdateInfiniteState();
			var healthElement = GetElement(nameof(CustomHealthValue));
			//var enableHealthElement = GetElement(nameof(EnableCustomHealth));
			//healthElement.Visible = !InFight && enableCustomHealth;
			healthElement.Order = GetElement(nameof(EnableCustomHealth)).Order + 1;

			GetElement(nameof(hardMode)).Visible = !InFight;
			//GetElement(nameof(EnableCustomHealth)).Visible = !InFight;
			GetElement(nameof(Infinite)).Visible = !InFight;
			//GetElement(nameof(IncreasedGodModeHealth)).Visible = !InFight && !Infinite;
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

		void UpdateInfiniteState()
		{
			//if (!InFight)
			//{
			var increasedHealthElement = GetElement(nameof(IncreasedGodModeHealth));
			increasedHealthElement.Visible = !InFight && !Infinite;

			var enableCustomHealthElement = GetElement(nameof(EnableCustomHealth));
			enableCustomHealthElement.Visible = !InFight && !Infinite;

			var pufferFishDifficultyElement = GetElement(nameof(PufferFishDifficulty));
			pufferFishDifficultyElement.Visible = !InFight && !Infinite;

			//var healthValueElement = GetElement("CustomHealthValue");
			GetElement(nameof(CustomHealthValue)).Visible = !InFight && !Infinite && EnableCustomHealth;
			//}



			/*if (Infinite)
			{
				healthValueElement.Visible = false;
			}
			else
			{
				healthValueElement.Visible = EnableCustomHealth;
			}*/
		}

		static void UpdateBlueState()
		{
			if (cameraMaterial == null)
			{
				cameraMaterial = WeaverAssets.LoadAssetFromBundle<Material>("infernogrimmmod", "CameraMaterial");
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
