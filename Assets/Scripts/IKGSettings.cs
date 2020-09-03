using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Configuration;

namespace Assets.Scripts
{
	public class IKGSettings : ModSettings
	{
		[Tooltip(@"Checking this will make the boss fight considerably harder

For those who want a bigger challenge")]
		public bool hardMode = false;

		[Tooltip(@"Checking this will allow you to customize the health to be whatever you want.

The value can be set below")]
		public bool enableCustomHealth = false;

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

	}
}
