﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Internal
{
	public static class InitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RunInitFunctions()
		{
			if (!run)
			{
				run = true;
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					RunInitFunctions(assembly);
				}
				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			RunInitFunctions(args.LoadedAssembly);
		}

		static void RunInitFunctions(Assembly assembly)
		{
			if (assembly.GetName().Name == "Assembly-CSharp")
			{
				return;
			}
			try
			{
				if (InitializedAssemblies.Contains(assembly))
				{
					return;
				}

				InitializedAssemblies.Add(assembly);

				foreach (var type in assembly.GetTypes().Where(t => typeof(IInit).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
				{
					var init = (IInit)Activator.CreateInstance(type);
					try
					{
						init.OnInit();
					}
					catch (Exception e)
					{
						WeaverLog.LogError("Init Error: " + e);
					}
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogError("Error when attempting to initialize [" + assembly.GetName().Name + "] : " + e);
			}
		}
	}
}
