using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{

	[Serializable]
	public class AssemblyInformation
	{
		public string AssemblyName;
		public AssemblyDefinitionFile Definition;
		public string AssemblyDefinitionPath;
		public string AssemblyGUID;
		public List<string> ScriptPaths;
	}


	[Serializable]
	public class ScriptInformation
	{
		public List<AssemblyInformation> AssemblyInfo;

		public static ScriptInformation GetProjectScriptInfo()
		{
			ScriptInformation info = new ScriptInformation();
			info.AssemblyInfo = new List<AssemblyInformation>();

			foreach (var (path, definition) in AssemblyDefinitionFile.GetAllDefinitionsInFolder("Assets"))
			{
				//Debug.Log("Adding Assembly = " + definition.name);
				info.AssemblyInfo.Add(new AssemblyInformation
				{
					AssemblyName = definition.name,
					AssemblyDefinitionPath = path,
					AssemblyGUID = AssetDatabase.GUIDFromAssetPath(path).ToString(),
					Definition = definition,
					ScriptPaths = new List<string>()
				});
			}

			//Debug.Log("Adding Assembly = Assembly-CSharp");
			info.AssemblyInfo.Add(new AssemblyInformation
			{
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp",
				ScriptPaths = new List<string>()
			});

			//Debug.Log("Adding Assembly = Assembly-CSharp");
			info.AssemblyInfo.Add(new AssemblyInformation
			{
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp-Editor",
				ScriptPaths = new List<string>()
			});

			var scriptIDs = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets" });
			foreach (var id in scriptIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(id);
				var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				var scriptAssembly = scriptAsset.GetScriptAssemblyName().Replace(".dll", "");

				//Debug.Log("Adding Script Under Assembly = " + scriptAssembly);



				info.AssemblyInfo.First(info => info.AssemblyName == scriptAssembly).ScriptPaths.Add(path);

				/*Debug.Log("Path = " + path);
				Debug.Log("Asset = " + asset.GetType());
				Debug.Log("Namespace = " + asset.GetScriptNamespace());
				Debug.Log("Assembly = " + asset.GetScriptAssemblyName());*/
			}

			/*var assemblyIDs = AssetDatabase.FindAssets("t:asmdef", new string[] { "Assets" });
			foreach (var assemblyID in assemblyIDs)
			{
				var assemblyAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assemblyID), typeof(TextAsset));
				Debug.Log("Assembly Type = " + assemblyAsset.GetType());
				foreach (var field in assemblyAsset.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					Debug.Log("Assembly Type Field = " + field.Name);
				}
			}*/

			/*var scriptIDs = AssetDatabase.FindAssets("t:MonoScript",new string[] { "Assets" });
			foreach (var id in scriptIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(id);
				var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				Debug.Log("Path = " + path);
				Debug.Log("Asset = " + asset.GetType());
				Debug.Log("Namespace = " + asset.GetScriptNamespace());
				Debug.Log("Assembly = " + asset.GetScriptAssemblyName());
			}*/

			/*var ids = AssetDatabase.FindAssets("TestAssembly");
			foreach (var id in ids)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(id),typeof(UnityEngine.Object));
				Debug.Log("Asset = " + asset.name);
				Debug.Log("Asset Type = " + asset.GetType());
				Debug.Log("GUID = " + id);
			}*/
			//AssetDatabase.FindAssets()

			return info;
		}
	}
}
