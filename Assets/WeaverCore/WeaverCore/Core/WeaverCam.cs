using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class WeaverCam : MonoBehaviour
	{
		//[OnRuntimeInit]
		[OnRegistryLoad]
		static void Init(Registry registry)
		{
			if (_instance == null)
			{
				WeaverLog.Log("STARTING UP CAMERA");
				_instance = staticImpl.Create();
				if (_instance == null)
				{
					WeaverLog.Log("Camera not set up yet");
					//UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
				}
				else
				{
					WeaverLog.Log("Initializing Camera");
					_instance.Initialize();
					foreach (var feature in Registry.GetAllFeatures<CameraExtension>())
					{
						Instantiate(feature, _instance.transform);
					}
				}
			}
			else
			{
				//registry.featu
				foreach (var feature in registry.GetFeatures<CameraExtension>())
				{
					Instantiate(feature, _instance.transform);
				}
			}
		}

		/*static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
			Init();
		}*/

		private static WeaverCam _instance = null;
		bool initialized = false;

		static WeaverCam_I.Statics staticImpl = ImplFinder.GetImplementation<WeaverCam_I.Statics>();
		WeaverCam_I impl;


		public CameraShaker Shaker { get; private set; }
		public static WeaverCam Instance
		{
			get
			{
				/*if (_instance == null)
				{
					Init();
				}*/
				return _instance;

			}
		}

		void Initialize()
		{
			if (initialized)
			{
				return;
			}

			impl = (WeaverCam_I)gameObject.AddComponent(ImplFinder.GetImplementationType<WeaverCam_I>());

			if (transform.parent == null)
			{
				var parentObject = new GameObject("Camera Parent");
				parentObject.transform.position = transform.position;
				Shaker = parentObject.AddComponent<CameraShaker>();
				transform.parent = parentObject.transform;
				transform.localPosition = Vector3.zero;
			}
			else
			{
				if ((Shaker = transform.parent.GetComponent<CameraShaker>()) == null)
				{
					Shaker = transform.parent.gameObject.AddComponent<CameraShaker>();
				}
			}

			impl.Initialize();

			initialized = true;
		}

		/*public PostProcessLayer AddPostProcessLayer(LayerMask mask)
		{
			var ppl = gameObject.AddComponent<PostProcessLayer>();
			ppl.volumeLayer = mask;
			ppl.

			return ppl;
		}*/
	}
}
