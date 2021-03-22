using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public class AssemblyDefinitionFile
	{
		public string name;
		public string rootNamespace = "";
		public List<string> references;
		public List<string> includePlatforms;
		public List<string> excludePlatforms;
		public bool allowUnsafeCode = false;
		public bool overrideReferences = false;
		public bool autoReferenced = true;
		public List<string> defineConstraints;
		public bool noEngineReferences = false;



		[SerializeField]
		List<string> precompiledReferences;

		public static AssemblyDefinitionFile Load(string path)
		{
			var asmDefAsset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
			if (asmDefAsset != null)
			{
				return JsonUtility.FromJson<AssemblyDefinitionFile>(asmDefAsset.text);
			}

			return null;
		}

		public static void Save(string path, AssemblyDefinitionFile asmDef)
		{
			//TextAsset asmDefAsset = new TextAsset(JsonUtility.ToJson(asmDef));
			//AssemblyDefinitionAsset asmDefAsset = (AssemblyDefinitionAsset)Activator.CreateInstance(typeof(AssemblyDefinitionAsset),BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,null,new object[] { JsonUtility.ToJson(asmDef) },null);
			//AssemblyDefinitionAsset asmDefAsset = new AssemblyDefinitionAsset();
			//AssetDatabase.CreateAsset(asmDefAsset, path);


			File.WriteAllText(path, JsonUtility.ToJson(asmDef));

		}

		public static IEnumerable<(string path, AssemblyDefinitionFile definition)> GetAllDefinitionsInFolder(string folder)
		{
			foreach (var id in AssetDatabase.FindAssets("t:asmdef",new string[] { folder }))
			{
				yield return (AssetDatabase.GUIDToAssetPath(id), Load(AssetDatabase.GUIDToAssetPath(id)));
			}
		}
	}
}
