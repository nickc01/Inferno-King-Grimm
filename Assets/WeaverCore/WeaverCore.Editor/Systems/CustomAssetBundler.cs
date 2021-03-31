using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Player;
using UnityEngine;

namespace WeaverCore.Editor
{
	public static class CustomAssetBundler
	{
		public class TestContent : BundleBuildParameters
		{
			public TestContent(BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
			{
				//Debug.Log("Constructor Target = " + target);
				//Debug.Log("Constructor Group = " + group);
				//Debug.Log("Constructor Output Folder = " + outputFolder);
				//this.
				//this.ContiguousBundles
			}

			public override UnityEditor.Build.Content.BuildSettings GetContentBuildSettings()
			{
				var settings = base.GetContentBuildSettings();
				//Debug.Log("Build Settings Type DB = " + settings.typeDB);
				//Debug.Log("Build Settings Target = " + settings.target);
				//Debug.Log("Build Settings Group = " + settings.group);
				//Debug.Log("Build Settings Flags = " + settings.buildFlags);
				return settings;
			}

			public override UnityEngine.BuildCompression GetCompressionForIdentifier(string identifier)
			{
				//Debug.Log("Compression Identifier = " + identifier);
				return base.GetCompressionForIdentifier(identifier);
			}

			public override string GetOutputFilePathForIdentifier(string identifier)
			{
				//Debug.Log("Output file Identifier = " + identifier);
				return base.GetOutputFilePathForIdentifier(identifier);
			}

			public override ScriptCompilationSettings GetScriptCompilationSettings()
			{
				return base.GetScriptCompilationSettings();
			}
		}

		public static IBundleBuildResults Test(BuildTarget target,BuildTargetGroup targetGroup, string outputFolder)
		{
			var buildContent = new BundleBuildContent(ContentBuildInterface.GenerateAssetBundleBuilds());
			var buildParams = new TestContent(target, targetGroup, outputFolder);
			//buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
			buildParams.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
			foreach (var files in buildContent.AdditionalFiles)
			{
				//Debug.Log("Additional File Under " + files.Key);
				foreach (var resource in files.Value)
				{
					//Debug.Log("Resource File Alias" + resource.fileAlias);
					//Debug.Log("Resource file name" + resource.fileName);
					//Debug.Log("Resource serialized file" + resource.serializedFile);
				}
			}

			foreach (var address in buildContent.Addresses)
			{
				//Debug.Log("Address GUID = " + address.Key);
				//Debug.Log("Address Value = " + address.Value);
			}

			foreach (var asset in buildContent.Assets)
			{
				//Debug.Log("Asset GUID = " + asset);
				//Debug.Log("Asset Path = " + AssetDatabase.GUIDToAssetPath(asset));
			}

			foreach (var layout in buildContent.BundleLayout)
			{
				//Debug.Log("Layout Value = " + layout.Key);
				foreach (var guid in layout.Value)
				{
					//Debug.Log("Layout GUID = " + guid);
					//Debug.Log("Layout Path = " + AssetDatabase.GUIDToAssetPath(guid));
				}
			}

			foreach (var custom in buildContent.CustomAssets)
			{
				//Debug.Log("Custom Asset GUID = " + custom.Asset);
				//Debug.Log("Processor? = " + custom.Processor.Method.DeclaringType.FullName + ":" + custom.Processor.Method.Name);
			}

			foreach (var scene in buildContent.Scenes)
			{
				//Debug.Log("Scene GUID = " + scene);
			}

			//DefaultBuildTasks.Create(DefaultBuildTasks.Preset.AssetBundleCompatible);

			//var result = PlayerBuildInterface.CompilePlayerScripts();

			//result.typeDB

			/*ContentPipeline.BuildCallbacks.PostScriptsCallbacks = (parameters, results) =>
			{
				//Debug.Log("POST SCRIPT CALLBACK");

				//Debug.Log("Assemblies Type = " + results.ScriptResults.assemblies.GetType());
				//Debug.Log("TYpeDB Type = " + results.ScriptResults.typeDB.GetType());

				var T_typeDB = results.ScriptResults.typeDB.GetType();

				var M_SerializeToJson = T_typeDB.GetMethod("SerializeToJson", BindingFlags.NonPublic | BindingFlags.Instance);

				var typeData = (string)M_SerializeToJson.Invoke(results.ScriptResults.typeDB, null);

				//ContentBuildInterface.ArchiveAndCompress()

				//File.WriteAllText("typeData.dat", typeData);

				//File.WriteAllText("testSerialization.json", EditorJsonUtility.ToJson(results.ScriptResults));
				//Debug.Log("ALL TYPE DATA = " + typeData);

				//var typeDB = results.ScriptResults.typeDB.GetObjectData()

				foreach (var writeResult in results.WriteResults)
				{
					//Debug.Log("Write Result for (" + writeResult.Key + ")");
					foreach (var type in writeResult.Value.includedTypes)
					{
						//Debug.Log("Included Type = " + type.FullName);
					}
				}
				return ReturnCode.Success;
			};*/

			/*ContentPipeline.BuildCallbacks.PostPackingCallback = (parameters,dependencies,writeData) =>
			{
				Debug.Log("POST PACKING CALLBACK");
				var helperType = typeof(AspectRatio).Assembly.GetType("UnityEditor.AssemblyHelper");

				var method = helperType.GetMethod("ExtractAssemblyTypeInfo");

				var assemblyInfo = (Array)method.Invoke(null, new object[] { false });
				foreach (var info in assemblyInfo)
				{
					var nameField = info.GetType().GetField("name");
					Debug.Log("Normal Assembly Name = " + nameField.GetValue(info));
				}

				assemblyInfo = (Array)method.Invoke(null, new object[] { true });
				foreach (var info in assemblyInfo)
				{
					var nameField = info.GetType().GetField("name");
					Debug.Log("Editor Assembly Name = " + nameField.GetValue(info));
				}
				//dependencies.AssetInfo;

				return ReturnCode.Success;
			};*/

			/*ContentPipeline.BuildCallbacks.PostDependencyCallback = (parameters,dependencies) =>
			{
				foreach (var info in dependencies.AssetInfo)
				{
					//Debug.Log("Dependency GUID = " + info.Key);
					//Debug.Log("Dependency GUID Path = " + AssetDatabase.GUIDToAssetPath(info.Key));
					//Debug.Log("Info Address = " + info.Value.address);
					//Debug.Log("Info GUID = " + info.Value.asset);
					//Debug.Log("info GUID Path = " + AssetDatabase.GUIDToAssetPath(info.Value.asset));
					foreach (var obj in info.Value.includedObjects)
					{
						if (!string.IsNullOrEmpty(obj.filePath))
						{
							//Debug.Log("Included Object = " + obj.filePath);
						}
					}
					foreach (var obj in info.Value.referencedObjects)
					{
						if (!string.IsNullOrEmpty(obj.filePath))
						{
							//Debug.Log("Referenced Object = " + obj.filePath);
						}
					}
				}
				return ReturnCode.Success;
			};*/

			/*ContentPipeline.BuildCallbacks.PostWritingCallback = (parameters, dependencies, writes,results) =>
			{
				foreach (var operation in writes.WriteOperations)
				{
					var refType = operation.ReferenceMap.GetType();

					var M_SerializeToJson = refType.GetMethod("SerializeToJson", BindingFlags.NonPublic | BindingFlags.Instance);

					var referenceMap = (string)M_SerializeToJson.Invoke(operation.ReferenceMap, null);

					//Debug.Log("Reference Map = " + referenceMap);
				}
				return ReturnCode.Success;
			};*/

			try
			{
				var code = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var results);

				switch (code)
				{
					case ReturnCode.Success:
						break;
					case ReturnCode.SuccessCached:
						break;
					case ReturnCode.SuccessNotRun:
						break;
					case ReturnCode.Error:
						throw new Exception("An error has occured during the compilation process");
					case ReturnCode.Exception:
						throw new Exception("An exception has occurred during the compilation process");
					case ReturnCode.Canceled:
						throw new Exception("The compilation has been cancelled");
					case ReturnCode.UnsavedChanges:
						throw new Exception("There are unsaved changes in the loaded scenes, be sure to save them");
					case ReturnCode.MissingRequiredObjects:
						throw new Exception("Missing Required Objects for compilation process");
					default:
						break;
				}

				return results;
			}
			finally
			{
				ContentPipeline.BuildCallbacks.PostScriptsCallbacks = null;
				ContentPipeline.BuildCallbacks.PostDependencyCallback = null;
				ContentPipeline.BuildCallbacks.PostPackingCallback = null;
				ContentPipeline.BuildCallbacks.PostWritingCallback = null;
			}

			//ContentPipeline.BuildAssetBundles(buildParams,buildContent,out var result,)

			/*ContentPipeline.BuildCallbacks.PostDependencyCallback += (buildParams, DependencyData) =>
			{
				return ReturnCode.Success;
			};*/

			//ContentPipeline.BuildAssetBundles()

			//CompatibilityBuildPipeline
			//BundleBuildContent
			//CompatibilityBuildPipeline

			//ContentPipeline.BuildCallbacks.PostDependencyCallback
		}
	}
}
