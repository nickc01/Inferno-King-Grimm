using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using System.Reflection;
using System;
using System.IO;
using WeaverCore.Helpers;

[Serializable]
public class DebugImage
{
	public Sprite sprite;
	public Texture texture;
}

public class EnemyTester : MonoBehaviour
{
	/*static void PrintMask(int mask)
	{
		for (int i = 0; i < 32; i++)
		{
			Debugger.Log("Bitwise = " + ((1 << i) & mask));
			if (((1 << i) & mask) == (1 << i))
			{
				Debugger.Log("Culling Layer " + i + " = " + LayerMask.LayerToName(i));
			}
		}
	}*/

	float CamZ;
	//SpriteRenderer renderer;
	class GMProps
	{
		public bool used = false;
	}

	PropertyTable<GameObject, GMProps> gmProperties = new PropertyTable<GameObject, GMProps>();

	void Start () 
	{
		//Debugger.Log("STARTING THE ENEMY REPLACEMENT GAMEOBJECT");

		SpriteRenderer renderer = GetComponent<SpriteRenderer>();

		var debugImage = new DebugImage()
		{
			sprite = renderer.sprite,
			texture = renderer.sprite.texture
		};

		var hijacker = GetComponent<EventReceiver>();

		hijacker.ReceiveAllEventsFromName("WAKE");

		hijacker.OnReceiveEvent += EnemyTester_OnReceiveEvent;

		//Debugger.Log("Serialized = " + JsonUtility.ToJson(debugImage));
		//var props = gmProperties.GetOrCreate(gameObject);
		//Debugger.Log("Used A = " + props.used);
		//props.used = true;


		//var props2 = gmProperties.GetOrCreate(gameObject);
		//Debugger.Log("Used B = " + props2.used);
		//TestHollowKnight.Class1.TestExecute();
		//renderer = GetComponent<SpriteRenderer>();
		//Debugger.Log("Enemy Tester Spawned!!!");

		//foreach (var component in gameObject.GetComponents<Component>())
		//{
		//Debugger.Log("Component = " + component.name);
		//Debugger.Log("Component Type = " + component.GetType());
		//if (component is SpriteRenderer)
		//{
		//var renderer = component as SpriteRenderer;
		//Debugger.Log("Material = " + renderer.material);
		//Debugger.Log("Shader = " + renderer.material.shader);
		//renderer.sortingLayerID = 1459018367;
		//renderer.sortingOrder = int.MaxValue - 1;
		//renderer.enabled = true;
		//Debugger.Log("Sprite = " + renderer.sprite);
		//if (renderer.sprite != null)
		//{
		//	Debugger.Log("Texture = " + renderer.sprite.texture);
		//}
		//}
		//}

		StartCoroutine(Waiter());
		//Debugger.Log("B");
		//var camera = Camera.main;
		//CamZ = camera.transform.position.z;
		//PrintMask(camera.cullingMask);
		//var cameraType = camera.GetType();
		/*foreach (var prop in cameraType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			try
			{
				var getter = prop.GetGetMethod();
				Debugger.Log(getter.Name + " = " + getter.Invoke(camera, null));
			}
			catch (Exception e)
			{
				Debugger.LogError("Prop Exception for " + cameraType.Name + " = " + e);
			}
		}

		foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>())
		{
			Debugger.Log("GameObject = " + gameObject.name);
			var Collider = gameObject.GetComponent<Collider2D>();
			if (Collider != null)
			{
				Debugger.Log("Bounds = " + Collider.bounds);
				Debugger.Log("Min = " + Collider.bounds.min);
				Debugger.Log("Max = " + Collider.bounds.max);
				Debugger.Log("Center = " + Collider.bounds.center);
				Debugger.Log("Size = " + Collider.bounds.size);
			}
		}*/
		/*var textureDumpFolder = Path.GetTempPath() + "TextureDump\\";
		Directory.CreateDirectory(textureDumpFolder);
		foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
		{
			try
			{
				var data = texture.EncodeToPNG();
				using (var file = File.Open(textureDumpFolder + texture.name + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
				{
					file.Write(data, 0, data.Length);
				}
				Debugger.Log("Texture = " + texture);
			}
			catch (Exception e)
			{

			}
		}*/
		//var cameraProps = JsonUtility.ToJson(camera);
		//cameraProps.
		//Debugger.Log("Scale = " + transform.localScale.x);
	}

	private void EnemyTester_OnReceiveEvent(string obj)
	{
		Debugger.Log("NEW ENEMY EVENT RECIEVED = " + obj);
	}

	void Update()
	{
		CamZ -= Time.deltaTime;
		var camera = Camera.main;
		transform.position = new Vector3(85,12,0);
		//transform.position = new Vector3(Player.Instance.transform.position.x, Player.Instance.transform.position.y, Player.Instance.transform.position.z);
		//transform.localScale = Player.Instance.transform.localScale;
		//Debugger.Log("Position = " + transform.position);
		//Debugger.Log("Scale = " + transform.localScale);
		//Debugger.Log("Player Scale = " + Player.Instance.transform.localScale);
		//transform.localScale = Player.Instance.transform.localScale;
		//Debugger.Log("Local Scale = " + transform.localScale);
		//Debugger.Log("Local Scale X = " + transform.localScale.x);
		//Debugger.Log("Local Scale Y = " + transform.localScale.y);
		//Debugger.Log("Local Scale Z = " + transform.localScale.z);
		//Debugger.Log("Player Position = " + Player.Instance.transform.position);
		//transform.position = new Vector3(camera.transform.position.x,camera.transform.position.y,CamZ);
	}

	IEnumerator Waiter()
	{
		for (int i = 0; i < 10; i++)
		{
			yield return new WaitForSeconds(1.0f);
			//Debugger.Log("Scale 2 = " + transform.localScale.x);

			//var playerRenderer = Player.Instance.GetComponent<SpriteRenderer>();

			//transform.localScale = playerRenderer.bounds.size;

			//Debugger.Log("Scale 2 = " + transform.localScale.x);
		}
		//yield return new WaitForSeconds(10.0f);
		//Debugger.Log("Done Waiting");
		Enemy.EndBossBattle(0.0f);
	}
}
