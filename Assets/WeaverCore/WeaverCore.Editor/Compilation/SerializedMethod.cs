using System;
using System.Reflection;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public sealed class SerializedMethod
	{
		public SerializedMethod()
		{

		}

		public SerializedMethod(MethodInfo method)
		{
			Method = method;
		}

		public string MethodName;
		public string TypeName;
		public string AssemblyName;

		public MethodInfo Method
		{
			get
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.FullName == AssemblyName)
					{
						var type = assembly.GetType(TypeName);
						if (type != null)
						{
							return type.GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
						}
					}
				}
				return null;
			}
			set
			{
				AssemblyName = value.DeclaringType.Assembly.FullName;
				TypeName = value.DeclaringType.FullName;
				MethodName = value.Name;
			}
		}
	}
}
