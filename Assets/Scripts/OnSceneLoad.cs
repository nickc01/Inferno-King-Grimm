using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.GodsOfGlory;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace Assets.Scripts
{
	class OnSceneLoad : IInit
	{
		public void OnInit()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			if (CoreInfo.LoadState == WeaverCore.Enums.RunningState.Game && arg0.name.Contains("GG_Workshop"))
			{
				GGWorkshop.ChangeStatue("GG_Statue_Grimm", "Inferno King Grimm");
			}
		}
	}

	//class OnSceneLoad : IInit
	//{
	/*public void OnInit()
	{
		//if (titleObject.GetComponent(TMProTextSetter.TMProT) != null)
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
	{
		Debugger.Log("New Scene = " + arg0.name);
		if (Core.LoadState == WeaverCore.Enums.RunningState.Game && arg0.name.Contains("GG_Workshop"))
		{

		}
	}*/

	//IEnumerator TextSetter(UnityEngine.SceneManagement.Scene scene)
	//{
	//yield return new WaitForSeconds(1f);
	/*foreach (var obj in scene.GetRootGameObjects())
	{
		if (obj.GetComponent(TMProTextSetterGUI.TMProUGUIT) != null)
		{
			Debugger.Log("Found tmProGUIObject = " + obj.name);
			var setter = obj.AddComponent<TMProTextSetterGUI>();
			Debugger.Log("Text = " + setter.Text);
			if (setter.Text == "Nightmare King Grimm")
			{
				setter.UpdateText = "Inferno King Grimm";
			}
		}
		if (obj.GetComponent(TMProTextSetter.TMProT) != null)
		{
			Debugger.Log("Found tmProObject = " + obj.name);
			var setter = obj.AddComponent<TMProTextSetter>();
			Debugger.Log("Text = " + setter.Text);
			if (setter.Text == "Nightmare King Grimm")
			{
				setter.UpdateText = "Inferno King Grimm";
			}
		}
	}*/
	//}
	//}
}
