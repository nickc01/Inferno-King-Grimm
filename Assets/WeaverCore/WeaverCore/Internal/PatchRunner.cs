﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
	public class PatchRunner : IInit
	{
		static HashSet<Assembly> PatchedAssemblies = new HashSet<Assembly>();

		public void OnInit()
		{
			//Debugger.Log("PATCHING ALL ASSEMBLIES = " + new System.Diagnostics.StackTrace(true));

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				PatchAssembly(assembly);
			}

			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}

		private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			PatchAssembly(args.LoadedAssembly);
		}

		void PatchAssembly(Assembly assembly)
		{
			if (CoreInfo.LoadState == RunningState.Game && assembly.GetName().Name == "Assembly-CSharp")
			{
				return;
			}
			try
			{
				if (PatchedAssemblies.Contains(assembly))
				{
					return;
				}

				PatchedAssemblies.Add(assembly);
				foreach (var type in assembly.GetTypes().Where(t => typeof(IPatch).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
				{
					var patcher = (IPatch)Activator.CreateInstance(type);

					var patcherInstance = HarmonyPatcher.Create("com." + assembly.GetName().Name + ".patch");

					try
					{
						patcher.Patch(patcherInstance);
					}
					catch (Exception e)
					{
						WeaverLog.LogError("Patch Error: " + e);
					}
				}
			}
			catch (Exception e)
			{
				//WeaverLog.LogError("Error when attempting to run patches for [" + assembly.GetName().Name + "] : " + e);
			}
		}
	}
}
