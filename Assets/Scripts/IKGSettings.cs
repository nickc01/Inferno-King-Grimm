﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Settings;

namespace Assets.Scripts
{
	public class IKGSettings : Panel
	{
		[Tooltip(@"Checking this will make the boss fight considerably harder

For those who want a bigger challenge")]
		[SettingField(Visibility.MenuOnly)]
		public bool hardMode = false;

		[Tooltip(@"Checking this will allow you to customize the health to be whatever you want.

The value can be set below")]
		[SerializeField]
		[SettingField(Visibility.Never)]
		bool enableCustomHealth = false;

		[SettingField(Visibility.MenuOnly)]
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
						GetElement("CustomHealthValue").Visible = enableCustomHealth;
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

	}
}
