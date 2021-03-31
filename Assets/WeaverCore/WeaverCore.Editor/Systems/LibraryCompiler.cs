using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.WriteTypes;
using UnityEditor.Build.Player;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Attributes;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Structures;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Systems
{
	public static class LibraryCompiler
	{
		public static bool BuildingAssemblies { get; private set; }
		public static bool BuildingBundles { get; private set; }

		static string AsmLocationRelative = "Assets\\WeaverCore\\WeaverCore.Editor";
		static DirectoryInfo AsmEditorLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor");

		public static IEnumerable<BuildTarget> MainTargets
		{
			get
			{
				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneWindows))
				{
					yield return BuildTarget.StandaloneWindows;
				}

				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneOSX))
				{
					yield return BuildTarget.StandaloneOSX;
				}

				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneLinux64))
				{
					yield return BuildTarget.StandaloneLinux64;
					//yield return BuildTarget.StandaloneLinuxUniversal;
				}
			}
		}

		[OnInit]
		static void Init()
		{
			if (WeaverReloadTools.DoReloadTools)
			{
				BuildStrippedWeaverCore();
			}
		}



		public static FileInfo DefaultWeaverCoreBuildLocation
		{
			get
			{
				return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");
			}
		}
		public static FileInfo DefaultAssemblyCSharpLocation
		{
			get
			{
				return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll");
			}
		}

		/// <summary>
		/// Builds both the HollowKnight assembly and a partial version of WeaverCore that does not contain any embedded resources. This is primarily used with WeaverCore.Game
		/// </summary>
		public static void BuildStrippedWeaverCore()
		{
			BuildStrippedWeaverCore(DefaultWeaverCoreBuildLocation.FullName);
		}

		public static void BuildStrippedWeaverCore(string buildLocation)
		{
			UnboundCoroutine.Start(BuildStrippedWeaverCoreAsync(buildLocation));
		}

		/// <summary>
		/// Builds a full version of WeaverCore that does contain embedded resources.
		/// </summary>
		/// <param name="buildLocation"></param>
		public static void BuildWeaverCore(string buildLocation)
		{

			File.Copy(DefaultWeaverCoreBuildLocation.FullName, buildLocation, true);

			var weaverGameLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo("Assets\\WeaverCore\\Libraries\\0Harmony.dll");
			var iLGenLocation = new FileInfo("Assets\\WeaverCore\\Libraries\\System.Reflection.Emit.ILGeneration.dll");
			var emitLocation = new FileInfo("Assets\\WeaverCore\\Libraries\\System.Reflection.Emit.dll");

			EmbedResourceCMD.EmbedResource(buildLocation, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, iLGenLocation.FullName, "ILGeneration", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, emitLocation.FullName, "ReflectionEmit", compression: CompressionMethod.NoCompression);
		}

		/// <summary>
		/// Builds only the bundles required for WeaverCore. The list will get populated when the function is done
		/// </summary>
		/// <param name="bundles"></param>
		/// <returns></returns>
		public static IEnumerator BuildWeaverCoreBundles(List<BundleBuild> bundles, IEnumerable<BuildTarget> buildTargets)
		{
			yield return BuildAssetBundles(bundles, null, buildTargets);
			for (int i = bundles.Count - 1; i >= 0; i--)
			{
				if (!bundles[i].File.Name.Contains(WeaverAssets.WeaverAssetBundleName))
				{
					bundles.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Builds both the mod bundles and the WeaverCore Bundles. The list will get populated when the function is done
		/// </summary>
		/// <param name="bundles"></param>
		/// <param name="modName"></param>
		/// <returns></returns>
		public static IEnumerator BuildAssetBundles(List<BundleBuild> bundles, string modName, IEnumerable<BuildTarget> buildTargets)
		{
			if (BuildingBundles)
			{
				yield break;
			}
			if (BuildingAssemblies)
			{
				yield return new WaitUntil(() => !BuildingAssemblies);
			}
			try
			{
				BuildingAssemblies = true;
				BuildingBundles = true;
				WeaverLog.Log("Beginning Bundling");
				PrepareForBundling(modName);
				var temp = Path.GetTempPath();
				var bundleBuilds = new DirectoryInfo(temp + "BundleBuilds\\");

				if (bundleBuilds.Exists)
				{
					bundleBuilds.Delete(true);
				}

				bundleBuilds.Create();

				//CustomAssetBundler.Test(BuildTarget.StandaloneWindows,BuildTargetGroup.Standalone,bundleBuilds.CreateSubdirectory(BuildTarget.StandaloneWindows.ToString() + "_NEW").FullName);

				foreach (var target in buildTargets)
				{
					var targetFolder = bundleBuilds.CreateSubdirectory(target.ToString());

					targetFolder.Create();
					CompatibilityBuildPipeline.BuildAssetBundles(targetFolder.FullName, BuildAssetBundleOptions.ChunkBasedCompression, target);
					//var results = CustomAssetBundler.Test(target, BuildTargetGroup.Standalone, targetFolder.FullName);
					foreach (var bundleFile in targetFolder.GetFiles())
					{
						if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
						{
							var newBundleFile = AssemblyReplacer.ReplaceAssembliesIn(bundleFile.FullName);
							bundles.Add(new BundleBuild() { File = new FileInfo(newBundleFile), Target = target});
						}
					}
				}
			}
			finally
			{
				BuildingAssemblies = false;
				BuildingBundles = false;
				AfterBundling(modName);
				WeaverLog.Log("Done Bundling");
			}
		}

		static void PrepareForBundling(string modName = null)
		{
			AssemblyReplacer.AssemblyReplacements.Add("HollowKnight.dll", "Assembly-CSharp.dll");
			//AssemblyReplacer.AssemblyRemovals.Add("WeaverCore.Editor.dll");
			if (modName != null)
			{
				AssemblyReplacer.AssemblyReplacements.Add("Assembly-CSharp.dll", modName + ".dll");
				//MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", modName);
				foreach (var registry in RegistryChecker.LoadAllRegistries())
				{
					registry.ReplaceAssemblyName("Assembly-CSharp", modName);
					registry.ApplyChanges();
				}
			}
			//MonoScriptUtilities.ChangeAssemblyName("HollowKnight", "Assembly-CSharp");
			//yield return SwitchToCompileMode();
		}

		static void AfterBundling(string modName = null)
		{
			AssemblyReplacer.AssemblyReplacements.Clear();
			//AssemblyReplacer.AssemblyRemovals.Clear();
			//MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", "HollowKnight");
			if (modName != null)
			{
				//MonoScriptUtilities.ChangeAssemblyName(modName, "Assembly-CSharp");
				foreach (var registry in RegistryChecker.LoadAllRegistries())
				{
					registry.ReplaceAssemblyName(modName, "Assembly-CSharp");
					registry.ApplyChanges();
				}
			}
			//SwitchToEditorMode();
		}

		static IEnumerator SwitchToCompileMode()
		{
			if (!File.Exists(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt"))
			{
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt");
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");
				AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");
				yield return null;
			}
		}

		static void SwitchToEditorMode()
		{
			//Debug.Log("Switching BACK");
			if (File.Exists(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt"))
			{
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt");
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");
				AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");
			}
		}

		static IEnumerator BuildHollowKnightASM(string buildLocation)
		{
			var assemblyCSharpBuilder = new Builder();
			assemblyCSharpBuilder.BuildPath = buildLocation;
			var scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\Hollow Knight");
			scripts.RemoveAll(f => f.FullName.Contains("Editor\\"));
			assemblyCSharpBuilder.Scripts = scripts;
			assemblyCSharpBuilder.Defines.Add("GAME_BUILD");

			AddUnityReferences(assemblyCSharpBuilder);
			//RemoveEditorReferences(assemblyCSharpBuilder);


			//var settings = GameBuildSettings.GetSettings();

			//var managedLocation = new DirectoryInfo(settings.HollowKnightLocation + "\\hollow_knight_Data\\Managed");
			//assemblyCSharpBuilder.ExcludedReferences.Add("UnityEngine.dll");

			//assemblyCSharpBuilder.ExcludedReferences.Add("Managed\\UnityEngine.dll");
			//assemblyCSharpBuilder.ExcludedReferences.Add("Managed/UnityEngine.dll");
			//assemblyCSharpBuilder.ExcludedReferences.Add(@"C:\Program Files\Unity\Hub\Editor\2020.2.2f1\Unity\Editor\Data\Managed\UnityEngine.dll");


			//var defaultReferences = 

			//assemblyCSharpBuilder.ExcludedReferences.Add("C:/Program Files/Unity/Hub/Editor/2020.2.2f1/Unity/Editor/Data/Managed/UnityEngine.dll");
			//assemblyCSharpBuilder.ExcludedReferences.Add("C:\\Program Files\\Unity\\Hub\\Editor\\2020.2.2f1\\Unity\\Editor\\Data\\Managed\\UnityEngine.dll");


			//Debug.Log("Location = " + typeof(MonoBehaviour).Assembly.Location);

			//var unityDLLDirectory = new DirectoryInfo(@"C:\Program Files\Unity\Hub\Editor\2020.2.2f1\Unity\Editor\Data\Managed\UnityEngine");

			//if (unityDLLDirectory.Parent.Exists && unityDLLDirectory.Parent.Name == "Managed")
			//{
			//Debug.Log("Removing UnityENGINE REFERENCE");
			//Debug.Log("Directory Path = " + unityDLLDirectory.Parent.FullName);
			//assemblyCSharpBuilder.ExcludedReferences.Add(PathUtilities.ReplaceSlashes(unityDLLDirectory.Parent.FullName + "\\UnityEngine.dll"));
			//assemblyCSharpBuilder.ExcludedReferences.Add(unityDLLDirectory.Parent.FullName + "\\UnityEngine.dll");
			//Debug.Log("ENGINE PATH = " + PathUtilities.ReplaceSlashes(unityDLLDirectory.Parent.FullName + "\\UnityEngine.dll"));
			//Debug.Log("ACTUAL PATH = " + "C:/Program Files/Unity/Hub/Editor/2020.2.2f1/Unity/Editor/Data/Managed/UnityEngine.dll");

			//assemblyCSharpBuilder.ExcludedReferences.Add("UnityEngine.dll");

			//assemblyCSharpBuilder.ExcludedReferences.Add("C:/Program Files/Unity/Hub/Editor/2020.2.2f1/Unity/Editor/Data/Managed/UnityEngine.dll");
			//}


			/*foreach (var hkFile in unityDLLDirectory.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				if (hkFile.Name.Contains("UnityEngine"))
				{
					//Debug.Log("Adding File = " + hkFile.FullName);
					assemblyCSharpBuilder.ReferencePaths.Add(hkFile.FullName);
				}
			}*/


			//var defaultReferences = assemblyCSharpBuilder.GetDefaultReferences().ToArray();


			/*foreach (var hkFile in managedLocation.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				//if (hkFile.Name != "UnityEngine.dll" && !hkFile.Name.Contains("Editor"))
				if (!hkFile.Name.Contains("Assembly-CSharp") && !hkFile.Name.Contains("MMHOOK") && hkFile.Name != "UnityEngine.dll")
				{
					if (!defaultReferences.Any(dRef => dRef.Name == hkFile.Name))
					{
						Debug.Log("Adding File = " + hkFile.FullName);
						//assemblyCSharpBuilder.ReferencePaths.Add(hkFile.FullName);
						//assemblyCSharpBuilder.ReferencePaths.Add(PathUtilities.ReplaceSlashes(hkFile.FullName));
					}
				}
			}*/

			//assemblyCSharpBuilder.ReferencePaths.Add("C:/Program Files/Unity/Hub/Editor/2020.2.2f1/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll");




			/*foreach (var hkFile in managedLocation.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				if (hkFile.Name == "UnityEngine.CoreModule.dll")
				{
					Debug.Log("Adding File = " + hkFile.FullName);
					assemblyCSharpBuilder.ReferencePaths.Add(hkFile.FullName);
				}
			}*/

			/*if (hkFile.Name != "UnityEngine.dll" && (hkFile.Name.Contains("Unity") || hkFile.Name.Contains("ConditionalExpression") || hkFile.Name == "PlayMaker.dll"))
{
	Debug.Log("Adding File = " + hkFile.FullName);
	assemblyCSharpBuilder.ReferencePaths.Add(hkFile.FullName);
}*/

			//Debug.Log("Hollow Knight Location = " + settings.HollowKnightLocation);

			//assemblyCSharpBuilder.ReferencePaths.Add("UnityEngine.CoreModule");
			assemblyCSharpBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
			//assemblyCSharpBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
			if (File.Exists(buildLocation))
			{
				File.Delete(buildLocation);
			}

			yield return assemblyCSharpBuilder.Build();
		}

		public static void RemoveEditorReferences(Builder builder)
		{
			var defaultReferences = builder.GetDefaultReferences();

			foreach (var dRef in defaultReferences)
			{
				var file = new FileInfo(dRef);
				if (file.Name.Contains("Editor"))
				{
					builder.ExcludedReferences.Add(dRef);
				}
			}
		}

		public static void AddUnityReferences(Builder builder)
		{
			var coreModuleLocation = new FileInfo(typeof(MonoBehaviour).Assembly.Location);
			var unityAssemblyFolder = coreModuleLocation.Directory;
			var managedFolder = unityAssemblyFolder.Parent;


			builder.ExcludedReferences.Add(managedFolder.FullName + @"\UnityEngine.dll");
			builder.ExcludedReferences.Add(PathUtilities.ReplaceSlashes(managedFolder.FullName + @"\UnityEngine.dll"));

			foreach (var hkFile in unityAssemblyFolder.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				if (hkFile.Name.Contains("UnityEngine"))
				{
					//Debug.Log("Adding File = " + hkFile.FullName);
					builder.ReferencePaths.Add(PathUtilities.ReplaceSlashes(hkFile.FullName));
				}
			}
		}

		/// <summary>
		/// Builds a partial version of WeaverCore and the HollowKnight assembly
		/// </summary>
		/// <param name="buildLocation">Where the build will be placed</param>
		/// <returns></returns>
		public static IEnumerator BuildStrippedWeaverCoreAsync(string buildLocation)
		{
			if (BuildingAssemblies)
			{
				yield return new WaitUntil(() => !BuildingAssemblies);
			}
			try
			{
				BuildingAssemblies = true;
				yield return BuildHollowKnightASM(DefaultAssemblyCSharpLocation.FullName);

				var weaverCoreBuilder = new Builder();
				weaverCoreBuilder.BuildPath = buildLocation;
				weaverCoreBuilder.Defines.Add("GAME_BUILD");
				weaverCoreBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\WeaverCore").Where(f => f.Directory.FullName.Contains(""));

				//For some reason, this only works when using forward slashes and not backslashes.
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");

				weaverCoreBuilder.ReferencePaths.Add(DefaultAssemblyCSharpLocation.FullName);

				//var unityDLLDirectory = new DirectoryInfo(@"C:\Program Files\Unity\Hub\Editor\2020.2.2f1\Unity\Editor\Data\Managed\UnityEngine");

				AddUnityReferences(weaverCoreBuilder);

				/*foreach (var hkFile in unityDLLDirectory.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
				{
					if (hkFile.Name.Contains("UnityEngine"))
					{
						//Debug.Log("Adding File = " + hkFile.FullName);
						weaverCoreBuilder.ReferencePaths.Add(hkFile.FullName);
					}
				}*/

				if (File.Exists(buildLocation))
				{
					File.Delete(buildLocation);
				}
				yield return weaverCoreBuilder.Build();
			}
			finally
			{
				BuildingAssemblies = false;
			}
		}
	}

	public static class AssemblyReplacer// : ClassWidePatcher
	{
		public static Dictionary<string, string> AssemblyReplacements = new Dictionary<string, string>();
		//public static List<string> AssemblyRemovals = new List<string>();

		/// <summary>
		/// Replaces the assembly names in a bundle and returns the new bundle file location
		/// </summary>
		/// <param name="bundleFileLocation">The input bundle</param>
		/// <returns>The output bundle</returns>
		public static string ReplaceAssembliesIn(string bundleFileLocation)
		{
			var am = new AssetsManager();
			am.LoadClassPackage("Assets\\WeaverCore\\Libraries\\classdata.tpk");

			var bun = am.LoadBundleFile(bundleFileLocation);
			//load the first entry in the bundle (hopefully the one we want)
			var assetsFileData = BundleHelper.LoadAssetDataFromBundle(bun.file, 0);
			var assetsFileName = bun.file.bundleInf6.dirInf[0].name; //name of the first entry in the bundle

			//I have a new update coming, but in the current release, assetsmanager
			//will only load from file, not from the AssetsFile class. so we just
			//put it into memory and load from that...
			var assetsFileInst = am.LoadAssetsFile(new MemoryStream(assetsFileData), "dummypath", false);
			var assetsFileTable = assetsFileInst.table;

			//load cldb from classdata.tpk
			am.LoadClassDatabaseFromPackage(assetsFileInst.file.typeTree.unityVersion);

			List<BundleReplacer> bundleReplacers = new List<BundleReplacer>();
			List<AssetsReplacer> assetReplacers = new List<AssetsReplacer>();

			foreach (var monoScriptInfo in assetsFileTable.GetAssetsOfType(0x73)) //loop over all monoscript assets
			{
				//get basefield
				var monoScriptInst = am.GetTypeInstance(assetsFileInst.file, monoScriptInfo).GetBaseField();
				var m_AssemblyNameValue = monoScriptInst.Get("m_AssemblyName").GetValue();
				foreach (var testAsm in AssemblyReplacements)
				{
					//"Assembly-CSharp.dll"
					if (m_AssemblyNameValue.AsString().Replace(".dll","") == testAsm.Key.Replace(".dll",""))
					{
						var newAsmName = testAsm.Value;

						if (!newAsmName.Contains(".dll"))
						{
							newAsmName += ".dll";
						}
						//change m_AssemblyName field
						m_AssemblyNameValue.Set(newAsmName);
						//rewrite the asset and add it to the pending list of changes
						assetReplacers.Add(new AssetsReplacerFromMemory(0, monoScriptInfo.index, (int)monoScriptInfo.curFileType, 0xffff, monoScriptInst.WriteToByteArray()));
						break;
					}
				}
			}

			//rewrite the assets file back to memory
			byte[] modifiedAssetsFileBytes;
			using (MemoryStream ms = new MemoryStream())
			using (AssetsFileWriter aw = new AssetsFileWriter(ms))
			{
				aw.bigEndian = false;
				assetsFileInst.file.Write(aw, 0, assetReplacers, 0);
				modifiedAssetsFileBytes = ms.ToArray();
			}

			//adding the assets file to the pending list of changes for the bundle
			bundleReplacers.Add(new BundleReplacerFromMemory(assetsFileName, assetsFileName, true, modifiedAssetsFileBytes, modifiedAssetsFileBytes.Length));

			byte[] modifiedBundleBytes;
			using (MemoryStream ms = new MemoryStream())
			using (AssetsFileWriter aw = new AssetsFileWriter(ms))
			{
				bun.file.Write(aw, bundleReplacers);
				modifiedBundleBytes = ms.ToArray();
			}

			using (MemoryStream mbms = new MemoryStream(modifiedBundleBytes))
			using (AssetsFileReader ar = new AssetsFileReader(mbms))
			{
				AssetBundleFile modifiedBundle = new AssetBundleFile();
				modifiedBundle.Read(ar);

				//recompress the bundle and write it (this is optional of course)
				using (FileStream ms = File.OpenWrite(bundleFileLocation + ".edit"))
				using (AssetsFileWriter aw = new AssetsFileWriter(ms))
				{
					bun.file.Pack(modifiedBundle.reader, aw, AssetBundleCompressionType.LZ4);
				}
			}

			return bundleFileLocation + ".edit";
			/*var assetManager = new AssetsManager();

			//var test = assetManager.LoadAssetsFile();

			//test.

			var bundleFile = assetManager.LoadBundleFile(bundleFileLocation, true);
			//bundleFile.

			//bundleFile.file

			var assets = BundleHelper.LoadAllAssetsFromBundle(bundleFile.file);

			assets[0].
			//bundleFile.assetsFiles[0].table.Get

			//assets[0].

			Debug.Log("Loaded Bundle = " + bundleFile.name);

			foreach (var file in bundleFile.assetsFiles)
			{
				if (file != null)
				{
					Debug.Log("Found Asset = " + file.name);
					//file.table.
				}
				else
				{
					Debug.Log("Found Asset = null");
				}
			}*/

			//foreach (var asset in assets)
			//{

			//asset.
			/*foreach (var assetInfo in asset.)
			{

			}*/
			//assetManager.GetTypeInstance(asset,)
			//}

			/*foreach (var asset in assets)
			{
				//asset.table.
				if (asset == null)
				{
					Debug.Log("Asset = Null");
				}
				else
				{
					//Debug.Log("Asset = " + asset.header.)
					//Debug.Log("Asset = " + asset.typeTree);
					//Debug.Log("Bundle = " + asset.parentBundle.name);
				}
				//asset.table.Get
				//Debug.Log("Asset" + asset.table.)
			}*/

			//assetManager
		}

		/*static bool EnableReplacements
		{
			get
			{
				return AssemblyReplacements.Count > 0;
			}
		}

		static bool EnableRemovals
		{
			get
			{
				return AssemblyRemovals.Count > 0;
			}
		}*/

		/*protected override Type GetClassToPatch()
		{
			return typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
		}*/

		[OnInit]
		static void Init()
		{
			//Debug.Log("APPLYING PATCHES");
			//var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
			//var compType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation");
			//var patcher = HarmonyPatcher.Create("com.AssemblyReplacer.patch");
			//patcher.Patch(type.GetMethod("GetTargetAssemblyInfos", BindingFlags.Public | BindingFlags.Static),null, typeof(AssemblyReplacer).GetMethod("PostfixGetTargetAssemblies"));
			//patcher.Patch(type.GetMethod("GetAllCompiledAndResolvedTargetAssemblies", BindingFlags.Public | BindingFlags.Static), null, typeof(AssemblyReplacer).GetMethod("PostfixGetTargetAssemblies"));
			//patcher.Patch(type.GetMethod("GetTargetAssembly",BindingFlags.Public | BindingFlags.Static),null, typeof(AssemblyReplacer).GetMethod("PostfixGetTargetAssembly"));

			//patcher.Patch(compType.GetMethod("CreateScriptAssembly"),null,typeof(AssemblyReplacer).GetMethod("CreateScriptAssembly_Postfix"));

			//patcher.Patch(typeof(LibraryCompiler).GetMethod("BuildHollowKnightASM", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance), typeof(AssemblyReplacer).GetMethod("PREFIX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),null);

			//var testMethod = typeof(AssemblyReplacer).GetMethod("TESTER_PREFIX", BindingFlags.Static | BindingFlags.NonPublic);

			/*foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				if (method.Name != "get_Instance")
				{
					patcher.Patch(method, testMethod, null);
				}
			}*/


			//var M_AddAssemblyInfo = typeof(TypeDB).GetMethod("AddAssemblyInfo", BindingFlags.NonPublic | BindingFlags.Instance);

			//patcher.Patch(M_AddAssemblyInfo, typeof(AssemblyReplacer).GetMethod(nameof(AddAssemblyInfoPrefix), BindingFlags.NonPublic | BindingFlags.Static), null);

			//var M_WriteSerializedFile_Internal = typeof(ContentBuildInterface).GetMethod("WriteSerializedFile_Internal", BindingFlags.NonPublic | BindingFlags.Static);
			//patcher.Patch(M_WriteSerializedFile_Internal, typeof(AssemblyReplacer).GetMethod(nameof(WriteSerializedFile_Internal_Prefix), BindingFlags.NonPublic | BindingFlags.Static), null);

			//var M_CreateWorkItems = typeof(UnityEditor.Build.Pipeline.Tasks.ArchiveAndCompressBundles).GetMethod("CreateWorkItems",BindingFlags.NonPublic | BindingFlags.Static);
			//patcher.Patch(M_CreateWorkItems, null, typeof(AssemblyReplacer).GetMethod(nameof(CreateWorkItems_Postfix), BindingFlags.NonPublic | BindingFlags.Static));
			//Debug.Log("APPLIED PATCH");

			//var M_Write = typeof(AssetBundleWriteOperation).GetMethod("Write");
			//patcher.Patch(M_Write, typeof(AssemblyReplacer).GetMethod(nameof(Write_Prefix), BindingFlags.NonPublic | BindingFlags.Static), null);


			//var M_ArchiveAndCompress = typeof(ContentBuildInterface).GetMethod("ArchiveAndCompress", new Type[] { typeof(ResourceFile[]), typeof(string), typeof(UnityEngine.BuildCompression) });
			//patcher.Patch(M_ArchiveAndCompress, typeof(AssemblyReplacer).GetMethod("ArchiveAndCompress_Prefix"), null);
			//Assembly test;

			//test.ful
			//test.name

			//Assembly test;

			//test.GetName()

			//Debug.Log("A");
			//var M_ASMFULLNAME = typeof(Assembly).GetProperty("FullName").GetGetMethod();
			//var M_ASMFULLNAME = typeof(Assembly).GetMethod("GetName", new Type[] { });
			//patcher.Patch(M_ASMFULLNAME, typeof(AssemblyReplacer).GetMethod(nameof(ASM_FULLNAME_PREFIX), BindingFlags.Static | BindingFlags.NonPublic), null);
			//Debug.Log("B");
			//M_ASMFULLNAME = typeof(Assembly).GetMethod("GetName", new Type[] { typeof(bool) });
			//patcher.Patch(M_ASMFULLNAME, typeof(AssemblyReplacer).GetMethod(nameof(ASM_FULLNAME_PREFIX), BindingFlags.Static | BindingFlags.NonPublic), null);
			//Debug.Log("C");

			/*foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var t in assembly.GetTypes())
				{
					if (typeof(Type).IsAssignableFrom(t) && !t.IsAbstract)
					{
						var typePatch = t.GetProperty(nameof(Type.AssemblyQualifiedName)).GetGetMethod();
						patcher.Patch(typePatch, typeof(AssemblyReplacer).GetMethod(nameof(QualifiedName_Prefix), BindingFlags.Static | BindingFlags.NonPublic), null);
						//Debug.Log("D");
						//Debug.Log(t.FullName + " is assignable to type");
					}
				}
			}*/
		}

		/// <summary>
		/// Finds how many of the <paramref name="sourceString"/> is inside of the <paramref name="stream"/>
		/// </summary>
		/// <param name="stream">The stream to search in</param>
		/// <param name="sourceString">The string to look for</param>
		/// <returns>Returns a list of where the occurances of <paramref name="sourceString"/> are positioned</returns>
		static List<long> SearchForStringInFile(FileStream stream, string sourceString)
		{
			long oldPosition = stream.Position;
			stream.Position = 0;
			try
			{
				//try
				//{
				List<long> replacementIndexes = new List<long>();

				var sourceChars = sourceString.ToCharArray();
				//var replacementChars = replacement.ToCharArray();


				//using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
				//{
				//using (var reader = new BinaryReader(stream,Encoding.UTF8))
				//{
				while (stream.Position != stream.Length)
				{
					long currentTestIndex = stream.Position;

					int currentValue = stream.ReadByte();

					if (currentValue == sourceChars[0])
					{
						//Debug.Log("Found a = " + sourceChars[0]);
						bool match = true;
						for (int i = 1; i < sourceChars.GetLength(0); i++)
						{
							if (stream.Position >= stream.Length)
							{
								match = false;
								break;
							}
							//else if ( != sourceChars[i])
							var nextChar = stream.ReadByte();
							//Debug.Log("Character After = " + nextChar);
							if (nextChar != sourceChars[i])
							{
								match = false;
								break;
							}
							//Debug.Log("Also Found a = " + sourceChars[i]);
						}
						if (match)
						{
							//Debug.Log("Match Found!!!");
							replacementIndexes.Add(currentTestIndex);
						}
						stream.Position = currentTestIndex + 1;
					}
				}
				//}
				//stream.Close();
				//}
				return replacementIndexes;
			}
			finally
			{
				stream.Position = oldPosition;
			}
		}

		/// <summary>
		/// Finds how many of the <paramref name="sourceString"/> is inside of the file at the <paramref name="filePath"/>
		/// </summary>
		/// <param name="filePath">The location of the file to search</param>
		/// <param name="sourceString">The string to look for</param>
		/// <returns>Returns a list of where the occurances of <paramref name="sourceString"/> are positioned</returns>
		static List<long> SearchForStringInFile(string filePath, string sourceString)
		{
			//try
			//{
			//List<long> replacementIndexes = new List<long>();

			//var sourceChars = sourceString.ToCharArray();
			//var replacementChars = replacement.ToCharArray();


			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
			{
				return SearchForStringInFile(stream, sourceString);
				//using (var reader = new BinaryReader(stream,Encoding.UTF8))
				//{
				/*while (stream.Position != stream.Length)
				{
					long currentTestIndex = stream.Position;

					int currentValue = stream.ReadByte();

					if (currentValue == sourceChars[0])
					{
						//Debug.Log("Found a = " + sourceChars[0]);
						bool match = true;
						for (int i = 1; i < sourceChars.GetLength(0); i++)
						{
							if (stream.Position >= stream.Length)
							{
								match = false;
								break;
							}
							//else if ( != sourceChars[i])
							var nextChar = stream.ReadByte();
							//Debug.Log("Character After = " + nextChar);
							if (nextChar != sourceChars[i])
							{
								match = false;
								break;
							}
							//Debug.Log("Also Found a = " + sourceChars[i]);
						}
						if (match)
						{
							//Debug.Log("Match Found!!!");
							replacementIndexes.Add(currentTestIndex);
						}
						stream.Position = currentTestIndex + 1;
					}
				}
				//}
				stream.Close();*/
			}
			//return replacementIndexes;
		}

		static int SearchAndReplaceInFile(string filePath, string sourceString, string replacement)
		{
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
			{
				return SearchAndReplaceInStream(stream,sourceString,replacement);
			}
		}

		static int SearchAndReplaceInStream(FileStream stream, string sourceString, string replacement)
		{
			var replacementArray = replacement.ToArray();

			long oldPosition = stream.Position;
			stream.Position = 0;
			try
			{
				var occurances = SearchForStringInFile(stream, sourceString);

				if (occurances.Count > 0)
				{
					using (var tempStream = new MemoryStream())
					{
						var nextOccurance = occurances[0];
						var occuranceIndex = 0;

						while (stream.Position != stream.Length)
						{
							if (stream.Position == nextOccurance)
							{
								for (int i = 0; i < replacement.Length; i++)
								{
									tempStream.WriteByte((byte)replacementArray[i]);
								}

								stream.Position += sourceString.Length;
								occuranceIndex++;
								if (occuranceIndex < occurances.Count)
								{
									nextOccurance = occurances[occuranceIndex];
								}
								else
								{
									nextOccurance = long.MaxValue;
								}

							}
							else
							{
								tempStream.WriteByte((byte)stream.ReadByte());
							}
						}

						stream.Position = 0;
						tempStream.Position = 0;

						while (tempStream.Position != tempStream.Length)
						{
							stream.WriteByte((byte)tempStream.ReadByte());
						}
						return occurances.Count;
					}
				}
				else
				{
					return 0;
				}
			}
			finally
			{
				stream.Position = oldPosition;
			}
		}

		static void CreateWorkItems_Postfix(object __result)
		{
			Debug.Log("Create Work Items Postfix!!!");
			var results = (IList)__result;

			foreach (var result in results)
			{
				var bundleName = result.GetType().GetField("BundleName").GetValue(result);
				Debug.Log("Bundle Name = " + bundleName);

				var outputFilePath = result.GetType().GetField("OutputFilePath").GetValue(result);
				Debug.Log("Output File Path = " + outputFilePath);

				var resourceFiles = (List<ResourceFile>)result.GetType().GetField("ResourceFiles").GetValue(result);
				foreach (var resource in resourceFiles)
				{
					Debug.Log("Resource File Name = " + resource.fileName);
					Debug.Log("Resource File Alias = " + resource.fileAlias);
					Debug.Log("Resource File is serialized = " + resource.serializedFile);

					//SearchAndReplaceInFile(resource.fileName, "HollowKnight.dll", "Assembly-CSharp.dll");
					//SearchAndReplaceInFile(resource.fileName, "HollowKnight.dll", "Assembly-CSharp.dll");

					SearchAndReplaceInFile(resource.fileName, "Assembly-CSharp.dll", "InfernoKingGrimm.dll");
					SearchAndReplaceInFile(resource.fileName, "Assembly-CSharp, ", "InfernoKingGrimm, ");
					SearchAndReplaceInFile(resource.fileName, "HollowKnight.dll", "Assembly-CSharp.dll");
					SearchAndReplaceInFile(resource.fileName, "HollowKnight, ", "Assembly-CSharp, ");

					//AssetsTools.NET.Extra.AssetsManager
					/*var occurances = SearchForStringInFile(resource.fileName, "HollowKnight.dll").Count;

					if (occurances > 0)
					{

					}*/
				}
			}
		}



		static bool ArchiveAndCompress_Prefix(ResourceFile[] resourceFiles, string outputBundlePath, UnityEngine.BuildCompression compression)
		{
			Debug.Log("ARCHIVING BUNDLES");

			foreach (var resource in resourceFiles)
			{
				Debug.Log("Resource File Name = " + resource.fileName);
				Debug.Log("Resource File Alias = " + resource.fileAlias);
				Debug.Log("Resource Is Serialized File = " + resource.serializedFile);
			}

			Debug.Log("Output Bundle Path = " + outputBundlePath);
			//Debug.Log("Compression = " + compression);

			return true;
		}

		static bool QualifiedName_Prefix(Type __instance)
		{
			//Debug.Log("Getting Qualified Name of Type = " + __instance.FullName);

			//Debug.Log("Qualified Assembly = " + __instance.Assembly.FullName);

			return true;
		}

		static bool ASM_FULLNAME_PREFIX(Assembly __instance)
		{
			//Debug.Log("Getting name of assembly = " + __instance.FullName);
			//Debug.Log("Getting FullName for Assembly = " + __instance.GetName().Name);
			return true;
		}

		static int counter = 0;

		static bool Write_Prefix(string outputFolder, UnityEditor.Build.Content.BuildSettings settings, BuildUsageTagGlobal globalUsage, AssetBundleWriteOperation __instance)
		{
			//Debug.Log("_-_-_DOING WRITING");
			//Debug.Log("_-_-_DOING WRITING");
			//Debug.Log("Output Folder = " + outputFolder);
			//Debug.Log("Write Command File Name = " + __instance.Command.fileName);
			//Debug.Log("Write Command Internal Name = " + __instance.Command.internalName);
			foreach (var sObject in __instance.Command.serializeObjects)
			{
				//Debug.Log("Serialization Index = " + sObject.serializationIndex);
				//Debug.Log("Serialization Object = " + sObject.serializationObject.ToString());
			}

			//Debug.Log("Build Settings Target = " + settings.target);
			//Debug.Log("Build Settings Group = " + settings.group);
			//Debug.Log("Build Settings Flags = " + settings.buildFlags);

			//PrintJsonData(__instance.UsageSet, "USAGE SET STUFF " + counter + ".json");
			//PrintJsonData(__instance.ReferenceMap, "REFERENCE MAP STUFF " + counter + ".json");

			/*foreach (var preload in preloadInfo.preloadObjects)
			{
				Debug.Log("Preload Object = " + preload);
			}*/

			//Debug.Log("Asset Bundle Name = " + __instance.Info.bundleName);

			counter++;
			return true;
			//return true;
		}

		/*static bool WriteSerializedFile_Internal_Prefix(string outputFolder,UnityEditor.Build.Content.BuildSettings settings,BuildUsageTagGlobal globalUsage, AssetBundleWriteOperation __instance)
		{
			
		}*/

		/*static void PrintJsonData(object obj, string fileName)
		{
			var type = obj.GetType();

			var M_SerializeToJson = type.GetMethod("SerializeToJson", BindingFlags.NonPublic | BindingFlags.Instance);

			File.WriteAllText(fileName, (string)M_SerializeToJson.Invoke(obj, null));
		}

		static bool AddAssemblyInfoPrefix(ref object assemblyInfos, ref string assembliesPath)
		{
			try
			{
				var asmInfoArray = (Array)assemblyInfos;

				foreach (var info in asmInfoArray)
				{
					Debug.Log("Info Name = " + info.GetType().GetField("name").GetValue(info));
					Debug.Log("Info Path = " + info.GetType().GetField("path").GetValue(info));
				}

				Debug.Log("Assemblies Path = " + assembliesPath);

				return true;
			}
			catch (Exception e)
			{
				Debug.LogError("Exception in Assembly Info Patch");
				Debug.LogException(e);
				return true;
			}
		}

		static bool TESTER_PREFIX(MethodBase __originalMethod)
		{
			Debug.Log("_+_+_+_RUNNING EDITOR METHOD = " + __originalMethod.Name);
			return true;
		}

		static bool PREFIX()
		{

			//Debug.Log("PREFIX WORKED!!!");
			return true;
		}


		public static void CreateScriptAssembly_Postfix(ref object __result)
		{

			Debug.Log("Create Script Assembly Postfix!!!");

			if (__result != null)
			{
				var type = __result.GetType();
				//Debug.Log("Result Type = " + type.FullName);

				var refProperty = type.GetProperty("References");

				string[] references = (string[])refProperty.GetValue(__result);

				foreach (var asmRef in references)
				{
					//Debug.Log("ACTUAL ASM REF = " + asmRef);
				}
			}
		}

		public static void PostfixGetTargetAssemblies(ref Array __result)
		{
			Debug.Log("Target Assemblies Test A");
			try
			{
				if (EnableReplacements)
				{
					//Debug.Log("DOING REPLACEMENTS");
					for (int i = 0; i < __result.GetLength(0); i++)
					{
						var asmInfo = __result.GetValue(i);

						var asmInfoType = asmInfo.GetType();

						var nameField = asmInfoType.GetField("Name");

						var name = (string)nameField.GetValue(asmInfo);

						if (AssemblyReplacements.ContainsKey(name))
						{
							nameField.SetValue(asmInfo, AssemblyReplacements[name]);
							__result.SetValue(asmInfo, i);
						}
					}
				}

				if (EnableRemovals)
				{
					//Debug.Log("DOING REMOVALS");
					//var oldList = __result;
					List<object> tempList = new List<object>();
					for (int i = 0; i < __result.GetLength(0); i++)
					{
						tempList.Add(__result.GetValue(i));
					}

					for (int i = tempList.Count - 1; i >= 0; i--)
					{
						//var asmInfo = __result.GetValue(i);
						var asmInfo = tempList[i];
						var asmInfoType = asmInfo.GetType();

						var nameField = asmInfoType.GetField("Name");
						var flagsField = asmInfoType.GetField("Flags");

						var name = (string)nameField.GetValue(asmInfo);

						//Debug.Log("BUNDLE SCRIPT = " + name);

						if (AssemblyRemovals.Contains(name))
						{
							//Debug.Log("REMOVING = " + name);

							tempList.RemoveAt(i);
							continue;

						}
					}

					var elementType = __result.GetValue(0).GetType();
					__result = Array.CreateInstance(elementType, tempList.Count);
					for (int i = 0; i < tempList.Count; i++)
					{
						__result.SetValue(tempList[i], i);
					}

					for (int i = 0; i < __result.GetLength(0); i++)
					{
						//Debug.Log("FINAL RESULT = " + __result.GetValue(i));
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error in GetTargetAssemblyInfos: " + e);
			}
		}

		public static void PostfixGetTargetAssembly(ref object __result, string scriptPath)
		{
			//Debug.Log("Target Assembly Test B");
			try
			{
				if (__result is string)
				{
					var assembly = (string)__result;
					if (EnableReplacements && AssemblyReplacements.ContainsKey(assembly))
					{
						__result = AssemblyReplacements[assembly];
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error in GetTargetAssembly: " + e);
			}
		}*/
	}
}
