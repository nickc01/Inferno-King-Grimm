using System;
using System.Collections.Generic;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public class ReplacedAssembly
	{
		/// <summary>
		/// The assembly to replace during the asset bundling
		/// </summary>
		public string AssemblyToReplace;
		/// <summary>
		/// The new name for the assembly
		/// </summary>
		public string NewAssemblyName;
	}

	[Serializable]
	public class EmbedInfo
	{
		/// <summary>
		/// The path to the file to embed
		/// </summary>
		public string FilePath;

		/// <summary>
		/// The name the file should be embedded under
		/// </summary>
		public string EmbedName;
	}

	[Serializable]
	public class ModBuildParameters
	{
		/// <summary>
		/// The name of the mod assembly to build
		/// </summary>
		public string AssemblyName;

		/// <summary>
		/// A list of scripts that are included in the build. Must be relative to the "Assets" directory
		/// </summary>
		public List<string> Scripts;

		/// <summary>
		/// A list of files to embed into the built assembly
		/// </summary>
		public List<EmbedInfo> FilesToEmbed;

		/// <summary>
		/// A list of assemblies the mod depends on
		/// </summary>
		public List<string> Dependencies;


		/// <summary>
		/// A list of assemblies to be excluded from the mod compilation
		/// </summary>
		public List<string> Exclusions;

		/// <summary>
		/// The path the final build of the mod will be placed
		/// </summary>
		public string BuildPath;

		/// <summary>
		/// Whether Unity assemblies should be referenced
		/// </summary>
		public bool IncludeUnityReferences = true;

		/// <summary>
		/// Whether editor references should be included
		/// </summary>
		public bool IncludeEditorReferences = false;

		/// <summary>
		/// A list of asset bundles to embed into the mod
		/// </summary>
		public List<string> BundlesToInclude = new List<string>();

	}


	[Serializable]
	public class BuildParameters
	{
		/*/// <summary>
		/// Assemblies to be built
		/// </summary>
		public List<string> AssembliesToBuild;*/

		/// <summary>
		/// Assemblies in this list will be excluded from the asset bundle compilation. This is done by temporarily converting them to editor only assemblies
		/// </summary>
		public List<string> ExcludedAssemblies;

		/// <summary>
		/// List of mod assemblies to rebuild
		/// </summary>
		public List<ModBuildParameters> Mods;

		/// <summary>
		/// A list of core mods that should be built before the other mods are
		/// </summary>
		public List<ModBuildParameters> CoreMods;

		/// <summary>
		/// A list of targets to build the asset bundles for
		/// </summary>
		public List<BuildTarget> Targets;

		/// <summary>
		/// The group to build the asset bundles for
		/// </summary>
		public BuildTargetGroup Group = BuildTargetGroup.Standalone;

		/// <summary>
		/// A list of all the assemblies to be changed during the asset bundling
		/// </summary>
		public List<ReplacedAssembly> ReplacedAssemblies;



		/*/// <summary>
		/// When building the asset bundles, any assembly names put in here will be redirected. For example, if you put
		/// </summary>
		public List<(string )> AssembliesToRename;*/


	}
}
