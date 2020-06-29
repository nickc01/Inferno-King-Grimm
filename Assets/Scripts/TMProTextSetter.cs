using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{


	public class TMProTextSetter : MonoBehaviour
	{
		private static Type tMProT = null;
		private static PropertyInfo textProperty = null;

		private Component tmPro;

		public static Type TMProT
		{
			get
			{
				Init();
				tMProT.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
				return tMProT;
			}
		}

		public static PropertyInfo TextProperty
		{
			get
			{
				Init();
				return textProperty;
			}
		}

		static void Init()
		{
			if (tMProT == null)
			{
				var mainAssembly = Assembly.Load("Assembly-CSharp");
				tMProT = mainAssembly.GetType("TMPro.TextMeshPro");
				textProperty = tMProT.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
			}
		}

		public string UpdateText = "";

		public string Text
		{
			get
			{
				Init();
				if (tmPro == null)
				{
					tmPro = GetComponent(tMProT);
				}
				return (string)textProperty.GetValue(tmPro, null);
			}
			private set
			{
				Init();
				if (tmPro == null)
				{
					tmPro = GetComponent(tMProT);
				}
				textProperty.SetValue(tmPro, value, null);
			}
		}

		void Update()
		{
			Init();
			if (tmPro == null)
			{
				tmPro = GetComponent(tMProT);
			}
			Text = UpdateText;
			//textProperty.SetValue(tmPro, Text, null);

		}
	}

	public class TMProTextSetterGUI : MonoBehaviour
	{
		private static Type tMProUGUIT = null;
		private static PropertyInfo textProperty = null;

		private Component tmProUGUI;

		public static Type TMProUGUIT
		{
			get
			{
				Init();
				return tMProUGUIT;
			}
		}

		public static PropertyInfo TextProperty
		{
			get
			{
				Init();
				return textProperty;
			}
		}

		static void Init()
		{
			if (tMProUGUIT == null)
			{
				var mainAssembly = Assembly.Load("Assembly-CSharp");
				tMProUGUIT = mainAssembly.GetType("TMPro.TextMeshPro");
				textProperty = tMProUGUIT.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
			}
		}

		public string UpdateText = "";

		public string Text
		{
			get
			{
				Init();
				if (tmProUGUI == null)
				{
					tmProUGUI = GetComponent(tMProUGUIT);
				}
				return (string)textProperty.GetValue(tmProUGUI, null);
			}
			private set
			{
				Init();
				if (tmProUGUI == null)
				{
					tmProUGUI = GetComponent(tMProUGUIT);
				}
				textProperty.SetValue(tmProUGUI, value, null);
			}
		}

		void Update()
		{
			Init();
			if (tmProUGUI == null)
			{
				tmProUGUI = GetComponent(tMProUGUIT);
			}
			Text = UpdateText;
			//textProperty.SetValue(tmProUGUI, Text, null);
		}
	}
}
