﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

internal struct HierarchicalData
{
	public bool ComponentEnabled;
	public Component PrefabComponent;
}

internal struct ComponentTypeData
{
	public Action<Component> AwakeFunction;
	public Action<Component> StartFunction;
	public ShallowCopyBuilder<Component>.ShallowCopyDelegate Copier;
}

namespace WeaverCore.Utilities
{
	public sealed class ObjectPool
	{
		private PoolLoadType _loadType;
		private InstanceStorage _storageLocation;
		private readonly Queue<PoolableObject> PooledObjects = new Queue<PoolableObject>();
		private readonly Dictionary<Type, ComponentTypeData> ComponentData = new Dictionary<Type, ComponentTypeData>();
		private readonly Dictionary<int, HierarchicalData> HierarchyData = new Dictionary<int, HierarchicalData>();

		public readonly PoolableObject Prefab;
		private readonly string InstanceName;

		public bool ResetComponents = true;
		public bool ResetPositions = false;

		public PoolLoadType LoadType
		{
			get
			{
				return _loadType;
			}
			set
			{
				if (_loadType != value)
				{
					_loadType = value;
					UpdateStorageLocation();
				}
			}
		}

		private InstanceStorage StorageLocation
		{
			get
			{
				if (_storageLocation == null)
				{
					UpdateStorageLocation();
				}
				return _storageLocation;
			}
		}

		public ObjectPool(PoolableObject prefab, PoolLoadType loadType = PoolLoadType.Local)
		{
			if (loadType == PoolLoadType.Global)
			{
				throw new Exception("A global pool is currently not supported yet");
			}
			if (prefab == null)
			{
				throw new NullReferenceException("The prefab passed into the pool is null");
			}
			Prefab = prefab;
			InstanceName = Prefab.gameObject.name + " (Clone)";
			_loadType = loadType;
			UpdateStorageLocation();
			ComponentPath[] components = Prefab.GetAllComponents();

			for (int i = 0; i < components.GetLength(0); i++)
			{
				ComponentPath componentPath = components[i];
				Type type = componentPath.Component.GetType();

				if (!ComponentData.ContainsKey(type))
				{
					ComponentTypeData cData = new ComponentTypeData();
					if (typeof(MonoBehaviour).IsAssignableFrom(type))
					{
						if (type != typeof(PoolableObject))
						{
							Action<Component> awake = GetAwakeDelegate(type);
							if (awake != null)
							{
								cData.AwakeFunction = awake;
							}
							Action<Component> start = GetStartDelegate(type);
							if (start != null)
							{
								cData.StartFunction = start;
							}
						}
						cData.Copier = CreateFieldCopier(type);
					}
					ComponentData.Add(type, cData);
				}

				int hierarchyHash = CreateHierarchyHash(componentPath);
				if (!HierarchyData.ContainsKey(hierarchyHash))
				{
					HierarchicalData hData = new HierarchicalData();
					if (componentPath.Component is Behaviour)
					{
						hData.ComponentEnabled = ((Behaviour)componentPath.Component).enabled;
					}
					else
					{
						hData.ComponentEnabled = true;
					}
					hData.PrefabComponent = componentPath.Component;
					HierarchyData.Add(hierarchyHash, hData);
				}
			}
		}

		public int AmountInPool
		{
			get
			{
				return PooledObjects.Count;
			}
		}

		private static T GetComponentOrThrow<T>(GameObject obj)
		{
			if (obj == null)
			{
				throw new NullReferenceException("The object passed in is null");
			}
			var c = obj.GetComponent<T>();
			if (c == null)
			{
				throw new Exception("The prefab [" + obj.name + "] does not have a [ " + typeof(T).FullName + " ] component");
			}
			return c;
		}

		private static T GetComponentOrThrow<T>(Component obj)
		{
			if (obj == null)
			{
				throw new NullReferenceException("The object passed in is null");
			}
			var c = obj.GetComponent<T>();
			if (c == null)
			{
				throw new Exception("The prefab [" + obj.name + "] does not have a [ " + typeof(T).FullName + " ] component");
			}
			return c;
		}

		private static int CreateHierarchyHash(ComponentPath componentPath)
		{
			int hierarchyHash = 0;
			HashUtilities.AdditiveHash(ref hierarchyHash, componentPath.SiblingHash);
			HashUtilities.AdditiveHash(ref hierarchyHash, componentPath.Component.GetType().GetHashCode());
			return hierarchyHash;
		}

		public ObjectPool(GameObject prefab, PoolLoadType loadType = PoolLoadType.Local) : this(GetComponentOrThrow<PoolableObject>(prefab), loadType) { }

		public ObjectPool(Component component, PoolLoadType loadType = PoolLoadType.Local) : this(GetComponentOrThrow<PoolableObject>(component), loadType) { }

		public void FillPool(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				PoolableObject obj = InstantiateNewObject();
				obj.startCalledAutomatically = true;
				obj.gameObject.SetActive(false);
			}
		}

		public void FillPoolAsync(int amount)
		{
			StorageLocation.StartCoroutine(FillPoolRoutine(amount));
		}

		public void ReturnToPool(GameObject gameObject)
		{
			ReturnToPool(gameObject, 0f);
		}

		public void ReturnToPool(PoolableObject poolableObject)
		{
			ReturnToPool(poolableObject, 0f);
		}

		public void ReturnToPool<T>(T component) where T : Component
		{
			ReturnToPool(component, 0f);
		}

		public void ReturnToPool(GameObject gameObject, float time)
		{
			if (gameObject == null)
			{
				//throw new NullReferenceException("Passed in gameObject is null");
				return;
			}
			PoolableObject poolComponent = gameObject.GetComponent<PoolableObject>();
			if (poolComponent == null)
			{
				throw new Exception("The object is not poolable. It does not contain a PoolableObject component");
			}
			ReturnToPool(poolComponent, time);
		}

		public void ReturnToPool(PoolableObject poolableObject, float time)
		{
			if (poolableObject == null)
			{
				//throw new NullReferenceException("Passed in object is null");
				return;
			}
			if (poolableObject.SourcePool != this)
			{
				throw new Exception("The passed in object did not originate from this pool");
			}

			if (time > 0)
			{
				poolableObject.StartCoroutine(ReturnToPoolRoutine(poolableObject, time));
				return;
			}

			SendBackToPool(poolableObject);
		}

		private void SendBackToPool(PoolableObject poolableObject)
		{
			ComponentPath[] objComponents = poolableObject.GetAllComponents();
			ComponentPath[] prefabComponents = Prefab.GetAllComponents();

			HashSet<int> PositionsSet = null;

			if (ResetPositions)
			{
				PositionsSet = new HashSet<int>();
			}

			for (int i = 0; i < objComponents.GetLength(0); i++)
			{
				ComponentPath componentPath = objComponents[i];
				Component component = componentPath.Component;
				Type type = component.GetType();

				if (component is IOnPool)
				{
					((IOnPool)component).OnPool();
				}
				if (component is MonoBehaviour)
				{
					((MonoBehaviour)component).StopAllCoroutines();
				}

				int hash = CreateHierarchyHash(componentPath);
				HierarchicalData hData;
				if (HierarchyData.TryGetValue(hash, out hData))
				{
					if (component is Behaviour)
					{
						((Behaviour)component).enabled = hData.ComponentEnabled;
					}

					ComponentTypeData cData;
					if (ComponentData.TryGetValue(type, out cData))
					{
						if (cData.Copier != null)
						{
							cData.Copier(component, hData.PrefabComponent);
						}
					}

					if (ResetPositions)
					{
						Transform t = component.transform;
						if (PositionsSet.Add(t.GetInstanceID()))
						{
							Transform prefabT = hData.PrefabComponent.transform;
							t.SetPositionAndRotation(prefabT.position, prefabT.rotation);
							t.localScale = prefabT.localScale;
						}
					}

				}
			}
			poolableObject.InPool = true;
			poolableObject.gameObject.SetActive(false);
			poolableObject.transform.parent = StorageLocation.transform;
			PooledObjects.Enqueue(poolableObject);
		}

		private IEnumerator ReturnToPoolRoutine(PoolableObject poolableObject, float time)
		{
			yield return new WaitForSeconds(time);
			if (poolableObject == null || poolableObject.gameObject == null)
			{
				yield break;
			}
			SendBackToPool(poolableObject);
		}

		public void ReturnToPool<T>(T component, float time) where T : Component
		{
			if (component == null)
			{
				//throw new NullReferenceException("Passed in component is null");
				return;
			}

			PoolableObject obj = null;
			if (component is PoolableObject)
			{
				obj = component as PoolableObject;
			}
			else
			{
				obj = component.GetComponent<PoolableObject>();
			}
			if (obj == null)
			{
				throw new Exception("The object is not poolable. It does not contain a PoolableObject component");
			}
			ReturnToPool(obj, time);
		}


		private PoolableObject InstantiateInternal(Vector3 position, Quaternion rotation, Transform parent)
		{
			PoolableObject obj = null;
			while (PooledObjects.Count > 0 && obj == null)
			{
				obj = PooledObjects.Dequeue();
			}

			//If there was a valid object in the queue
			if (obj != null)
			{
				obj.gameObject.name = InstanceName;
				obj.SourcePool = this;
				obj.InPool = false;
				Transform t = obj.transform;
				t.parent = parent;
				t.position = position;
				t.rotation = rotation;
				obj.gameObject.SetActive(Prefab.gameObject.activeSelf);

				ComponentPath[] objComponents = obj.GetAllComponents();

				for (int i = 0; i < objComponents.GetLength(0); i++)
				{
					ComponentPath componentPath = objComponents[i];
					Component component = componentPath.Component;
					Type cType = component.GetType();

					ComponentTypeData cData;
					if (ComponentData.TryGetValue(cType, out cData))
					{
						if (cData.AwakeFunction != null)
						{
							cData.AwakeFunction(component);
						}
					}
				}
				if (obj.startCalledAutomatically)
				{
					obj.startCalledAutomatically = false;
				}
				else
				{
					StorageLocation.AddStartCallerEntry(objComponents, ComponentData);
				}
				return obj;
			}
			else
			{
				obj = GameObject.Instantiate(Prefab, StorageLocation.transform);
				obj.SourcePool = this;
				obj.gameObject.name = InstanceName;
				Transform t = obj.transform;
				t.parent = parent;
				t.position = position;
				t.rotation = rotation;
				return obj;
			}
		}

		public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent)
		{
			return InstantiateInternal(position, rotation, parent).gameObject;
		}

		public GameObject Instantiate(Transform parent)
		{
			return InstantiateInternal(Prefab.transform.position, Prefab.transform.rotation, parent).gameObject;
		}

		public GameObject Instantiate()
		{
			return InstantiateInternal(Prefab.transform.position, Prefab.transform.rotation, null).gameObject;
		}

		public GameObject Instantiate(Vector3 position, Quaternion rotation)
		{
			return InstantiateInternal(position,rotation,null).gameObject;
		}

		public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace)
		{
			if (instantiateInWorldSpace)
			{
				return InstantiateInternal(Prefab.transform.position, Prefab.transform.rotation, parent).gameObject;
			}
			else
			{
				return InstantiateInternal(Prefab.transform.position + parent.transform.position, parent.transform.rotation * Prefab.transform.rotation, parent).gameObject;
			}
		}

		public T Instantiate<T>(Vector3 position, Quaternion rotation, Transform parent)
		{
			var instance = InstantiateInternal(position, rotation, parent);
			var component = instance.GetCommonComponent<T>();
			if (component == null)
			{
				throw new Exception("The objects in this pool do not have a " + typeof(T).FullName + " component");
			}
			return component;
		}

		public T Instantiate<T>(Transform parent)
		{
			return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, parent);
		}

		public T Instantiate<T>()
		{
			return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, null);
		}

		public T Instantiate<T>(Vector3 position, Quaternion rotation)
		{
			return Instantiate<T>(position, rotation, null);
		}

		public T Instantiate<T>(Transform parent, bool instantiateInWorldSpace)
		{
			if (instantiateInWorldSpace)
			{
				return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, parent);
			}
			else
			{
				return Instantiate<T>(Prefab.transform.position + parent.transform.position, parent.transform.rotation * Prefab.transform.rotation, parent);
			}
		}

		private IEnumerator FillPoolRoutine(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				PoolableObject obj = InstantiateNewObject();
				obj.startCalledAutomatically = true;
				obj.gameObject.SetActive(false);
				yield return null;
			}
		}

		private PoolableObject InstantiateNewObject()
		{
			PoolableObject instance = GameObject.Instantiate(Prefab, StorageLocation.transform);
			instance.gameObject.name = InstanceName;
			PooledObjects.Enqueue(instance);
			return instance;
		}

		private ShallowCopyBuilder<Component>.ShallowCopyDelegate CreateFieldCopier(Type componentType)
		{
			ShallowCopyBuilder<Component> copier = new ShallowCopyBuilder<Component>(componentType);

			foreach (FieldInfo field in componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (!field.IsDefined(typeof(ExcludeFieldFromPoolAttribute), false) && !field.IsInitOnly && !field.IsLiteral)
				{
					if (field.FieldType.IsValueType || field.FieldType.IsEnum)
					{
						copier.AddField(field);
					}
					else if (field.FieldType.IsClass && (field.IsPublic || field.IsDefined(typeof(SerializeField), true)))
					{
						if (!typeof(Component).IsAssignableFrom(field.FieldType) && !typeof(GameObject).IsAssignableFrom(field.FieldType))
						{
							copier.AddField(field);
						}
					}
				}
			}
			return copier.Finish();
		}

		private void UpdateStorageLocation()
		{
			if (_storageLocation == null)
			{
				if (LoadType == PoolLoadType.Global)
				{
					_storageLocation = InstanceStorage.GlobalStorage;
				}
				else
				{
					_storageLocation = InstanceStorage.LocalStorage;
				}
			}
			else
			{
				InstanceStorage newLocation = null;
				if (LoadType == PoolLoadType.Global && _storageLocation != InstanceStorage.GlobalStorage)
				{
					newLocation = InstanceStorage.GlobalStorage;
				}
				else if (LoadType == PoolLoadType.Local && _storageLocation != InstanceStorage.LocalStorage)
				{
					newLocation = InstanceStorage.LocalStorage;
				}
				if (newLocation != null)
				{
					if (_storageLocation != null)
					{
						for (int i = 0; i < PooledObjects.Count; i++)
						{
							PoolableObject obj = PooledObjects.Dequeue();
							obj.transform.SetParent(newLocation.transform);
							PooledObjects.Enqueue(obj);
						}
					}
					_storageLocation = newLocation;
				}
			}
		}

		private static Action<Component> GetAwakeDelegate(Type sourceType)
		{
			return GetDelegateForMethod(sourceType, sourceType.GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.HasThis, new Type[] { }, null));
		}

		private static Action<Component> GetStartDelegate(Type sourceType)
		{
			return GetDelegateForMethod(sourceType, sourceType.GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.HasThis, new Type[] { }, null));
		}

		private static Action<Component> GetDelegateForMethod(Type sourceType, MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			DynamicMethod awakeCaller = new DynamicMethod("", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, null, new Type[1] { typeof(Component) }, sourceType, true);
			ILGenerator gen = awakeCaller.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Castclass, sourceType);
			if (!sourceType.IsSealed && method.IsVirtual && !method.IsFinal)
			{

				gen.EmitCall(OpCodes.Call, method, null);
			}
			else
			{
				gen.EmitCall(OpCodes.Callvirt, method, null);
			}
			gen.Emit(OpCodes.Ret);
			return (Action<Component>)awakeCaller.CreateDelegate(typeof(Action<Component>));
		}
	}

	internal class InstanceStorage : MonoBehaviour
	{
		private struct StartCallerEntry
		{
			public readonly ComponentPath[] Components;
			public readonly Dictionary<Type, ComponentTypeData> ComponentData;

			public StartCallerEntry(ComponentPath[] components, Dictionary<Type, ComponentTypeData> componentData)
			{
				Components = components;
				ComponentData = componentData;
			}
		}

		static bool addedEvent = false;
		private Queue<StartCallerEntry> StartCallers;
		private static InstanceStorage _local;
		public static InstanceStorage LocalStorage
		{
			get
			{
				if (_local == null || _local.gameObject == null)
				{
					_local = new GameObject("Active Scene Pool").AddComponent<InstanceStorage>();
					if (!addedEvent)
					{
						addedEvent = true;
						SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
					}
				}
				return _local;
			}
		}

		private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
			_local = null;
			/*_local = new GameObject("Active Scene Pool").AddComponent<InstanceStorage>();
			if (_local.gameObject.scene.name != arg1.name)
			{
				SceneManager.MoveGameObjectToScene(_local.gameObject, arg1);
			}*/
		}

		private static InstanceStorage _global;
		public static InstanceStorage GlobalStorage
		{
			get
			{
				if (_global == null || _global.gameObject == null)
				{
					_global = new GameObject("Global Scene Pool").AddComponent<InstanceStorage>();
					DontDestroyOnLoad(_global.gameObject);
				}
				return _global;
			}
		}

		public void AddStartCallerEntry(ComponentPath[] componentsToCall, Dictionary<Type, ComponentTypeData> componentData)
		{
			if (StartCallers == null)
			{
				StartCallers = new Queue<StartCallerEntry>();
			}
			StartCallers.Enqueue(new StartCallerEntry(componentsToCall, componentData));
		}

		private void LateUpdate()
		{
			if (StartCallers != null)
			{
				while (StartCallers.Count > 0)
				{
					StartCallerEntry caller = StartCallers.Dequeue();
					for (int i = 0; i < caller.Components.GetLength(0); i++)
					{
						ComponentPath componentPath = caller.Components[i];
						Component component = componentPath.Component;
						if (component != null)
						{
							Type cType = component.GetType();
							ComponentTypeData cData;

							if (caller.ComponentData.TryGetValue(cType, out cData))
							{
								if (cData.StartFunction != null)
								{
									cData.StartFunction(component);
								}
							}
						}
					}
				}
			}
		}

		private Transform _transform;
		public new Transform transform
		{
			get
			{
				if (_transform == null)
				{
					_transform = GetComponent<Transform>();
				}
				return _transform;
			}
		}
	}
}
