using System;
using System.Reflection;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	public static class MonoScriptExtensions
	{
		static Type monoScriptType;
		static MethodInfo M_GetAssemblyName;
		static MethodInfo M_GetNamespace;


		static MonoScriptExtensions()
		{
			monoScriptType = typeof(MonoScript);
			M_GetAssemblyName = monoScriptType.GetMethod("GetAssemblyName", BindingFlags.NonPublic | BindingFlags.Instance);
			M_GetNamespace = monoScriptType.GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
		}


		public static string GetScriptAssemblyName(this MonoScript script)
		{
			return (string)M_GetAssemblyName.Invoke(script, null);
		}

		public static string GetScriptNamespace(this MonoScript script)
		{
			return (string)M_GetNamespace.Invoke(script, null);
		}
	}
}
