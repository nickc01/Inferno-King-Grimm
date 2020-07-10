using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

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
			if (text != null && textToSet != null)
			{
				text.text = textToSet;
			}
		}
	}
}
