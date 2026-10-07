using System;
using System.Collections;
using UnityEngine;

public class Routiner : MonoBehaviour
{
	private static Routiner instance;

	private Action onUpdate;

	private const float DelayEpsilon = 0.0001f;

	public static void Init()
	{
		if (!instance || !instance.gameObject)
		{
			instance = new GameObject("_routine").AddComponent<Routiner>();
			StaticObjectsManager.AddObject(instance.gameObject, false);
		}
	}

	public static Coroutine Go(IEnumerator routine)
	{
		Init();
		return instance.StartCoroutine(routine);
	}

	public static void Stop(Coroutine coroutine)
	{
		if (!(instance == null) && coroutine != null)
		{
			instance.StopCoroutine(coroutine);
		}
	}

	public static void AddUpdate(Action action)
	{
		Init();
		Routiner routiner = instance;
		routiner.onUpdate = (Action)Delegate.Combine(routiner.onUpdate, action);
	}

	private void Update()
	{
		if (instance.onUpdate != null)
		{
			instance.onUpdate();
		}
	}

	public static Coroutine GoDelayed(Action action, float delay)
	{
		Init();
		return instance.StartCoroutine(instance.DelayedRoutine(action, delay));
	}

	private IEnumerator DelayedRoutine(Action action, float delay)
	{
		IEnumerator enumerator = WaitRealtime(delay);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
		action();
	}

	public static Coroutine GoDelayed(IEnumerator routine, float delay)
	{
		Init();
		return instance.StartCoroutine(instance.DelayedRoutine(routine, delay));
	}

	private IEnumerator DelayedRoutine(IEnumerator routine, float delay)
	{
		IEnumerator enumerator = WaitRealtime(delay);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
		while (routine.MoveNext())
		{
			yield return routine.Current;
		}
	}

	private static IEnumerator WaitRealtime(float seconds)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup <= realtimeSinceStartup + seconds + 0.0001f)
		{
			yield return null;
		}
	}
}
