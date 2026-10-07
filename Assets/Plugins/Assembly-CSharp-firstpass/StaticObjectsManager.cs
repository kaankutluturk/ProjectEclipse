using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class StaticObjectsManager : MonoBehaviour
{
	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Action OnApplicationQuitEvent;

	private static StaticObjectsManager instance;

	private Transform _transform;

	private HashSet<GameObject> _staticObjects;

	protected static StaticObjectsManager Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static event Action ApplicationQuitting
	{
		add
		{
			add_OnApplicationQuitEvent(value);
		}
		remove
		{
			remove_OnApplicationQuitEvent(value);
		}
	}

	public static void add_OnApplicationQuitEvent(Action value)
	{
		Action action = OnApplicationQuitEvent;
		Action action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnApplicationQuitEvent, (Action)Delegate.Combine(action2, value), action);
		}
		while ((object)action != action2);
	}

	public static void remove_OnApplicationQuitEvent(Action value)
	{
		Action action = OnApplicationQuitEvent;
		Action action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnApplicationQuitEvent, (Action)Delegate.Remove(action2, value), action);
		}
		while ((object)action != action2);
	}

	protected static StaticObjectsManager GetInstance()
	{
		if (instance == null)
		{
			GameObject gameObject = new GameObject("STATIC_OBJECTS");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			instance = gameObject.AddComponent<StaticObjectsManager>();
			instance._transform = gameObject.transform;
			instance._staticObjects = new HashSet<GameObject>();
		}
		return instance;
	}

	public static void AddObject(GameObject gameObject, bool attachToManager = true)
	{
		if (!GetInstance()._staticObjects.Contains(gameObject))
		{
			if (attachToManager)
			{
				gameObject.transform.parent = GetInstance()._transform;
			}
			else
			{
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			if (gameObject.name[0] != '_')
			{
				gameObject.name = "_" + gameObject.name;
			}
			GetInstance()._staticObjects.Add(gameObject);
		}
	}

	public static void RemoveObject(GameObject gameObject)
	{
		if (GetInstance()._staticObjects.Contains(gameObject))
		{
			GetInstance()._staticObjects.Remove(gameObject);
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public static void Clear()
	{
		foreach (GameObject item in GetInstance()._staticObjects)
		{
			if ((bool)item)
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		if ((bool)GetInstance())
		{
			UnityEngine.Object.Destroy(GetInstance().gameObject);
		}
		instance = null;
	}

	private void OnApplicationQuit()
	{
		OnApplicationQuitEvent.SafeInvoke();
	}
}
