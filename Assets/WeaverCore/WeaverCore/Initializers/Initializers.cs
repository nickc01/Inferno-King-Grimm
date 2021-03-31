using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Initializers
{
	internal static class Initializers
	{
		internal static void OnGameInitialize() //This is only called when Hollow Knight starts up
		{
			if (CoreInfo.LoadState == Enums.RunningState.Game)
			{
				OnInitRunner.RunInitFunctions();
			}
		}

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void OnEditorInitialize() //This is only called when the editor initializes
		{
			OnInitRunner.RunInitFunctions();
		}
#endif

#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod]
#else
		[OnInit(int.MaxValue)]
#endif
		static void OnGamePlay() //This is called either when you go into play mode in the editor, or start up the game when in Hollow Knight
		{
			//WeaverLog.Log("BEGINNING OF RUNTIME INITS");
			RuntimeInitRunner.RuntimeInit();
		}
	}


	static class OnInitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RunInitFunctions()
		{
			if (!run)
			{
				run = true;
				//WeaverLog.Log("RUNNING INITS");

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InitializedAssemblies.Add(assembly);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>();

				//ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>(typeof(WeaverCore.Internal.WeaverCore).Assembly);
				//WeaverLog.Log("DONE RUNNING INITS");
				//AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		/*private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>(args.LoadedAssembly);
			}
		}*/
	}

	static class OnHarmonyPatchRunner
	{
		[OnInit(int.MaxValue / 2)]
		static void OnInit()
		{
			//WeaverLog.Log("RUNNING PATCHES");
			//WeaverLog.Log("AA");
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				//WeaverLog.Log("AB");
				PatchAssembly(assembly);
			}

			//WeaverLog.Log("DONE RUNNING PATCHES");
			//AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}

		/*private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			PatchAssembly(args.LoadedAssembly);
		}*/

		static void PatchAssembly(Assembly assembly)
		{
			//WeaverLog.Log("AC");
			var patcherInstance = HarmonyPatcher.Create("com." + assembly.GetName().Name + ".patch");
			//WeaverLog.Log("AD");
			var patchParameters = new Type[] { typeof(HarmonyPatcher) };
			//WeaverLog.Log("AE");
			var patchArguments = new object[] { patcherInstance };

			var inits = ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters).ToList();
			inits.Sort(new PriorityAttribute.PrioritySorter<OnHarmonyPatchAttribute>());

			foreach (var initPair in ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters))
			{
				try
				{
					initPair.Item1.Invoke(null, patchArguments);
				}
				catch (Exception e)
				{
					WeaverLog.LogError("Patch Error: " + e);
				}
			}
		}
	}

	static class RuntimeInitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RuntimeInit()
		{
			if (!run)
			{
				run = true;
				//WeaverLog.Log("RUNNING RUNTIME INITS");
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InitializedAssemblies.Add(assembly);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>();

				//WeaverLog.Log("DONE RUNNING RUNTIME INITS");

				//AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		/*private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>(args.LoadedAssembly);
			}
		}*/
	}
}
