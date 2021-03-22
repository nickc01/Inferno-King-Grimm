using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public class ModBuildResults
	{
		/// <summary>
		/// The path to the built mod
		/// </summary>
		public string Path;
	}

	[Serializable]
	public class BuildData
	{
		public const string BuildDataPath = "Assets\\WeaverCore\\Hidden~\\BuildData.dat";
		public static BuildData Data { get; private set; }


		public CurrentCompilationStep CurrentStep;
		[SerializeField]
		SerializedMethod nextMethodToRun;
		[SerializeField]
		BuildParameters parameters;
		[SerializeField]
		ScriptInformation scriptInfo;

		[SerializeField]
		bool doDebugBuild = false;



		[OnInit(int.MaxValue)]
		static void BuildInit()
		{
			if (File.Exists(BuildDataPath))
			{
				Data = JsonUtility.FromJson<BuildData>(File.ReadAllText(BuildDataPath));
			}
			else
			{
				Data = new BuildData();
			}

			Data.scriptInfo = ScriptInformation.GetProjectScriptInfo();


			if (Data.CurrentStep != CurrentCompilationStep.None)
			{
				if (Data.nextMethodToRun != null)
				{

					Debug.Log("Next Method To Run = " + Data.nextMethodToRun.Method?.Name);
					var method = Data.nextMethodToRun.Method;
					Data.nextMethodToRun = null;
					SaveData();
					if (method != null)
					{
						method.Invoke(null, null);
					}
				}
			}


			string mod = "InferoKingGrimm";
			//Data.scriptInfo = ScriptInformation.GetProjectScriptInfo();

			//Debug.Log("Script Info = " + JsonUtility.ToJson(Data.scriptInfo, true));

			//SaveData();

			var gameBuildSettings = GameBuildSettings.GetSettings();

			Debug.Log("Hollow Knight Location = " + gameBuildSettings.HollowKnightLocation);

			if (Data.doDebugBuild)
			{
				StartBuild(new BuildParameters
				{
					Targets = new List<BuildTarget>
					{
						BuildTarget.StandaloneWindows,
						BuildTarget.StandaloneOSX,
						BuildTarget.StandaloneLinux64
					},
					ExcludedAssemblies = new List<string>
					{
						"WeaverCore.Editor"
					},
					//BuildFolder = gameBuildSettings.HollowKnightLocation + "\\hollow_knight_Data\\Managed\\Mods",
					CoreMods = new List<ModBuildParameters>
					{
						new ModBuildParameters
						{
							AssemblyName = "Assembly-CSharp",
							Scripts = GetScriptsFromAssembly("HollowKnight"),
							BuildPath = "Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll",
							Exclusions = new List<string>
							{
								"Library/ScriptAssemblies/HollowKnight.dll"
							}

						},
						new ModBuildParameters
						{
							AssemblyName = "WeaverCore",
							BuildPath = "Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll",
							Scripts = GetScriptsFromAssembly("WeaverCore"),
							Exclusions = new List<string>
							{
								"Library/ScriptAssemblies/WeaverCore.dll",
								"Library/ScriptAssemblies/HollowKnight.dll"
							},
							Dependencies = new List<string>
							{
								"Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll"
							},
							FilesToEmbed = new List<EmbedInfo>
							{
								new EmbedInfo {EmbedName = "WeaverCore.Game", FilePath = "Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll"},
								new EmbedInfo {EmbedName = "0Harmony", FilePath = "Assets\\WeaverCore\\Libraries\\0Harmony.dll"},
								new EmbedInfo {EmbedName = "ILGeneration", FilePath = "Assets\\WeaverCore\\Libraries\\System.Reflection.Emit.ILGeneration.dll"},
								new EmbedInfo {EmbedName = "ReflectionEmit", FilePath = "Assets\\WeaverCore\\Libraries\\System.Reflection.Emit.dll"}
							},
							BundlesToInclude = new List<string>
							{
								"weavercore_bundle"
							},

						}
					},
					Mods = new List<ModBuildParameters>
					{
						new ModBuildParameters
						{
							AssemblyName = mod,
							BuildPath = gameBuildSettings.HollowKnightLocation + "\\hollow_knight_Data\\Managed\\Mods\\InferoKingGrimm.dll",
							Scripts = GetScriptsFromAssembly("Assembly-CSharp"),
							Exclusions = new List<string>
							{
								"Library/ScriptAssemblies/WeaverCore.dll",
								"Library/ScriptAssemblies/HollowKnight.dll"
							},
							BundlesToInclude = new List<string>
							{
								"inferokinggrimm_bundle"
							},
							Dependencies = new List<string>
							{
								"Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll",
								"Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll"
							}
						}
					}
				});
			}
		}

		public static List<string> GetScriptsFromAssembly(string assemblyName)
		{
			assemblyName = assemblyName.Replace(".dll", "");
			if (Data == null)
			{
				throw new Exception("The Build Data has not been initialized yet");
			}
			foreach (var assembly in Data.scriptInfo.AssemblyInfo)
			{
				if (assembly.AssemblyName == assemblyName)
				{
					return new List<string>(assembly.ScriptPaths);
				}
			}
			return null;
		}

		static void SaveData()
		{
			if (Data != null)
			{
				File.WriteAllText(BuildDataPath, JsonUtility.ToJson(Data,true));
			}
		}

		static void QueueMethodAfterReload(MethodInfo method)
		{
			//Debug.Log("SETTING METHOD To = " + method.Name);
			Data.nextMethodToRun = new SerializedMethod(method);
		}

		static void QueueMethodAfterReload(Type type, string methodName)
		{
			QueueMethodAfterReload(type.GetMethod(methodName,BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic));
		}

		static void QueueMethodAfterReload(string methodName)
		{
			QueueMethodAfterReload(typeof(BuildData),methodName);
			SaveData();
		}

		public static void StartBuild(BuildParameters parameters)
		{
			if (Data == null)
			{
				throw new Exception("Cannot start a build when the Build Data has not been filled yet");
			}

			if (Data.CurrentStep != CurrentCompilationStep.None)
			{
				throw new Exception("The build system is already building something");
			}

			Data.parameters = parameters;
			Data.CurrentStep = CurrentCompilationStep.AssemblyExclusion;
			Data.scriptInfo = ScriptInformation.GetProjectScriptInfo();
			int assembliesChanged = ExcludeAssemblies(Data.parameters.ExcludedAssemblies);

			if (assembliesChanged > 0)
			{
				Debug.Log("QUEUEING NEXT PART");
				QueueMethodAfterReload(nameof(BuildPart2));
			}
			else
			{
				BuildPart2();
			}
		}

		static void BuildPart2()
		{
			//Data.nextMethodToRun = null;
			Debug.Log("MOVING TO SECOND PART!!!");


		}

		/// <summary>
		/// Excludes assemblies from the compilation process later
		/// </summary>
		/// <param name="assembliesToExclude">A list of assembly names to exclude</param>
		/// <returns>Returns how many assemblies have been modified.</returns>
		static int ExcludeAssemblies(List<string> assembliesToExclude)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				int changedAssemblies = 0;
				foreach (var assembly in assembliesToExclude)
				{
					var asmName = assembly.Replace(".dll", "");
					var assemblyInfo = Data.scriptInfo.AssemblyInfo.FirstOrDefault(a => a.AssemblyName == asmName);
					if (assemblyInfo != null)
					{
						if (!assemblyInfo.Definition.includePlatforms.Contains("Editor"))
						{
							changedAssemblies++;
							assemblyInfo.Definition.includePlatforms.Add("Editor");
							AssemblyDefinitionFile.Save(assemblyInfo.AssemblyDefinitionPath, assemblyInfo.Definition);
							AssetDatabase.ImportAsset(assemblyInfo.AssemblyDefinitionPath);
						}
					}
				}
				return changedAssemblies;
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		static ModBuildResults BuildMod(ModBuildParameters parameters)
		{
			return null;
		}



	}
}
