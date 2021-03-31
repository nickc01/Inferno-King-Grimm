using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Internal
{
	static class ModLoader
	{
		class ModSorter : IComparer<WeaverMod>
		{
			Comparer<int> numberComparer;

			public ModSorter()
			{
				numberComparer = Comparer<int>.Default;
			}

			public int Compare(WeaverMod x, WeaverMod y)
			{
				return numberComparer.Compare(x.LoadPriority(), y.LoadPriority());
			}
		}


		[OnRuntimeInit(int.MaxValue)]
		static void RuntimeInit()
		{
			//WeaverLog.Log("Runtime INIT!!!");
			List<WeaverMod> mods = new List<WeaverMod>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach (var type in assembly.GetTypes())
					{
						if (typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
						{
							//WeaverLog.Log("FOUND MOD = " + type);
							var mod = (WeaverMod)Activator.CreateInstance(type);
							//mod.Initialize();
							mods.Add(mod);
						}
					}
				}
				catch (Exception e)
				{
					WeaverLog.Log("Error Creating Mod [" + assembly.GetName().Name + " : " + e);
				}
			}

			/*for (int i = 0; i < mods.Count; i++)
			{
				WeaverLog.Log("List Before = " + mods[i].GetType().Name);
			}*/
			//mods.Ord
			mods.Sort(new ModSorter());

			/*for (int i = 0; i < mods.Count; i++)
			{
				WeaverLog.Log("List After = " + mods[i].GetType().Name);
			}*/

			//foreach (var mod in mods)
			for (int i = 0; i < mods.Count; i++)
			{
				try
				{
					//WeaverLog.Log("PRIORITY = " + mods[i].LoadPriority());
					//WeaverLog.Log("LOADING MOD = " + mods[i].GetType());
					mods[i].Initialize();
				}
				catch (Exception e)
				{
					WeaverLog.LogError("Error Loading Mod " + mods[i].GetType());
					WeaverLog.LogError(e);
				}
			}
		}
	}
}
