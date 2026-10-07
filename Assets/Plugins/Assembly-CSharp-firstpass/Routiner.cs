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

	public static Coroutine Go(IEnumerator DCOLKHNLFNI)
	{
		Init();
		return instance.StartCoroutine(DCOLKHNLFNI);
	}

	public static void Stop(Coroutine DCOLKHNLFNI)
	{
		if (!(instance == null) && DCOLKHNLFNI != null)
		{
			instance.StopCoroutine(DCOLKHNLFNI);
		}
	}

	public static void AddUpdate(Action IBODMPMJELJ)
	{
		Init();
		Routiner eDAPJLKMFPC = instance;
		eDAPJLKMFPC.onUpdate = (Action)Delegate.Combine(eDAPJLKMFPC.onUpdate, IBODMPMJELJ);
	}

	private void Update()
	{
		if (instance.onUpdate != null)
		{
			instance.onUpdate();
		}
	}

	public static Coroutine GoDelayed(Action IBODMPMJELJ, float IHDMLLNEGIK)
	{
		Init();
		return instance.StartCoroutine(instance.DelayedRoutine(IBODMPMJELJ, IHDMLLNEGIK));
	}

	private IEnumerator DelayedRoutine(Action IBODMPMJELJ, float IHDMLLNEGIK)
	{
		IEnumerator enumerator = WaitRealtime(IHDMLLNEGIK);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
		IBODMPMJELJ();
	}

	public static Coroutine GoDelayed(IEnumerator DCOLKHNLFNI, float IHDMLLNEGIK)
	{
		Init();
		return instance.StartCoroutine(instance.DelayedRoutine(DCOLKHNLFNI, IHDMLLNEGIK));
	}

	private IEnumerator DelayedRoutine(IEnumerator DCOLKHNLFNI, float IHDMLLNEGIK)
	{
		IEnumerator enumerator = WaitRealtime(IHDMLLNEGIK);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
		while (DCOLKHNLFNI.MoveNext())
		{
			yield return DCOLKHNLFNI.Current;
		}
	}

	private static IEnumerator WaitRealtime(float IHDMLLNEGIK)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup <= realtimeSinceStartup + IHDMLLNEGIK + 0.0001f)
		{
			yield return null;
		}
	}
}
