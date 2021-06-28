using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore;
using WeaverCore.Settings;

namespace Assets.Scripts
{
	/*public class TMProTextSetter : MonoBehaviour
	{
		//public InfernoKingGrimm grimm;
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
			var settings = Panel.GetSettings<IKGSettings>();
			if (text.text.ToLower().Contains("grimm"))
			{
				if (text.text.Contains("Nightmare"))
				{
					var replacement = "Inferno";
					if (settings.hardMode)
					{
						replacement = "Absolute " + replacement;
					}
					//WeaverLog.Log("Replacing Text = " + text.text + " with " + text.text.Replace("Nightmare", "Inferno"));
					text.text = text.text.Replace("Nightmare", replacement);
					//WeaverLog.Log("Text Color =" + text.color);
					//WeaverLog.Log("Change 1");
				}



				if (settings.Infinite)
				{
					if (!text.text.StartsWith("Infinite"))
					{
						if (text.text.Contains("Infinite"))
						{
							if (text.text.ToLower().Contains("god"))
							{
								text.text = text.text.Replace("Infinite", "");
							}
							else
							{
								var replacement = "Inferno King";
								if (settings.hardMode)
								{
									replacement = "Absolute " + replacement;
								}
								text.text = text.text.Replace("Infinite", replacement);
							}
						}
						text.text = "Infinite " + text.text;
					}
				}
				else
				{
					if (text.text.Contains("Infinite"))
					{
						if (text.text.ToLower().Contains("god"))
						{
							text.text = text.text.Replace("Infinite", "");
						}
						else
						{
							var replacement = "Inferno King";
							if (settings.hardMode)
							{
								replacement = "Absolute " + replacement;
							}
							text.text = text.text.Replace("Infinite", replacement);
						}
					}
				}

				
				if (settings.Infinite && !text.text.Contains("Infinite"))
				{
					text.text = "Infinite " + text.text;
				}
				if (settings.hardMode)
				{
					if (text.color.r > 0.7f && text.color.g < 0.2f && text.color.b < 0.2f)
					{
						text.color = new Color(text.color.b, text.color.g, text.color.r, text.color.a);
					}
				}
			}
		}
	}*/
}
