using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore;

namespace Assets.Scripts
{
	public class TMProTextSetter : MonoBehaviour
	{
		TMP_Text text;

		public string textToSet = null;

		void Awake()
		{
			text = GetComponent<TMP_Text>();
			SetText();
		}

		void Update()
		{
			SetText();
		}

		void SetText()
		{

			if (text.text.Contains("Nightmare"))
			{
				WeaverLog.Log("Replacing Text = " + text.text + " with " + text.text.Replace("Nightmare", "Inferno"));
				text.text = text.text.Replace("Nightmare", "Inferno");
				WeaverLog.Log("Change 1");
			}
			if (text.text.Contains("Infinite"))
			{
				WeaverLog.Log("Replacing Text = " + text.text + " with " + text.text.Replace("Infinite", "Inferno King"));
				text.text = text.text.Replace("Infinite", "Inferno King");
				WeaverLog.Log("Change 2");
			}
			/*if (text != null && textToSet != null)
			{
				text.text = textToSet;
			}*/
		}
	}
}
