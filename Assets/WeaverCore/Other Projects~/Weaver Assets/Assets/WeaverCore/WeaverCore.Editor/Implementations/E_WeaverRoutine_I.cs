﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Implementations
{
	public class E_WeaverRoutine_I : WeaverRoutine_I
    {
		static List<IEnumerator<IWeaverAwaiter>> Routines = new List<IEnumerator<IWeaverAwaiter>>();

		static bool started = false;
		static bool initializedTimers = false;
		static float previousTime = 0;
		static float time = 0;

		public override float DT
		{
			get
			{
				return time - previousTime;
			}
		}

		public override URoutineData Start(IEnumerator<IWeaverAwaiter> function)
        {
            if (!started)
            {
                started = true;
                EditorApplication.update += OnUpdate;
            }
			if (function.MoveNext())
			{
				Routines.Add(function);
			}
			return new Data()
			{
				Function = function
			};
		}

        public override void Stop(URoutineData routine)
        {
			var data = (Data)routine;
			Routines.Remove(data.Function);
		}

		static void OnUpdate()
		{
			if (!initializedTimers)
			{
				initializedTimers = true;
				previousTime = (float)EditorApplication.timeSinceStartup;
				time = (float)EditorApplication.timeSinceStartup;
			}
			previousTime = time;
			time = (float)EditorApplication.timeSinceStartup;

			//float dt = time - previousTime;

			for (int i = Routines.Count - 1; i >= 0; i--)
			{
				if (Routines.Count > 0 && i >= Routines.Count)
				{
					i = Routines.Count - 1;
				}
				var routine = Routines[i];
				try
				{
					var current = routine.Current;
					if (current == null || !current.KeepWaiting())
					{
						if (!routine.MoveNext())
						{
							Routines.Remove(routine);
						}
					}
				}
				catch (Exception)
				{
					Routines.Remove(routine);
					throw;
				}
			}
		}

		class Data : URoutineData
		{
			public IEnumerator<IWeaverAwaiter> Function;
		}

	}
}
