using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
	private class CoroutineEntry
	{
		private static Dictionary<IEnumerator, CoroutineEntry> _Coroutines = new Dictionary<IEnumerator, CoroutineEntry>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IEnumerator _routine;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool _isRunning;

		public IEnumerator CoroutineEnumerator
		{
			get
			{
				return GetRoutine();
			}
			private set
			{
				set_Routine(value);
			}
		}

		public bool IsEntryRunning
		{
			get
			{
				return GetIsRunning();
			}
			private set
			{
				set_IsRunning(value);
			}
		}

		public object CurrentItem
		{
			get
			{
				return GetCurrent();
			}
		}

		public CoroutineEntry(IEnumerator routine)
		{
			set_Routine(routine);
			set_IsRunning(true);
			_Coroutines.Add(routine, this);
		}

		public static CoroutineEntry Find(IEnumerator routine)
		{
			CoroutineEntry value = null;
			_Coroutines.TryGetValue(routine, out value);
			return value;
		}

		public IEnumerator GetRoutine()
		{
			return _routine;
		}

		private void set_Routine(IEnumerator value)
		{
			_routine = value;
		}

		public bool GetIsRunning()
		{
			return _isRunning;
		}

		private void set_IsRunning(bool value)
		{
			_isRunning = value;
		}

		public bool MoveNext()
		{
			if (GetRoutine() != null && GetRoutine().MoveNext())
			{
				return true;
			}
			Stop();
			return false;
		}

		public void Stop()
		{
			if (GetIsRunning())
			{
				set_IsRunning(false);
				_Coroutines.Remove(GetRoutine());
			}
		}

		public object GetCurrent()
		{
			return GetRoutine().Current;
		}
	}

	private static CoroutineManager _Current;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _isPaused;

	public static CoroutineManager Instance
	{
		get
		{
			return get_Current();
		}
	}

	public bool ManagerPaused
	{
		get
		{
			return get_IsPaused();
		}
		private set
		{
			SetIsPaused(value);
		}
	}

	public static CoroutineManager get_Current()
	{
		if (_Current == null)
		{
			_Current = new GameObject("[CoroutineManager]").AddComponent<CoroutineManager>();
			Object.DontDestroyOnLoad(_Current.gameObject);
		}
		return _Current;
	}

	public bool get_IsPaused()
	{
		return _isPaused;
	}

	private void SetIsPaused(bool value)
	{
		_isPaused = value;
	}

	public void StartRoutine(IEnumerator routine)
	{
		CoroutineEntry entry = new CoroutineEntry(routine);
		StartCoroutine(RunRoutine(entry));
	}

	public void StopRoutine(IEnumerator routine)
	{
		CoroutineEntry entry = CoroutineEntry.Find(routine);
		if (entry != null)
		{
			entry.Stop();
		}
		StopCoroutine(routine);
	}

	private IEnumerator RunRoutine(CoroutineEntry entry)
	{
		yield return null;
		while (entry.GetIsRunning())
		{
			if (get_IsPaused())
			{
				yield return null;
			}
			else if (entry.MoveNext())
			{
				yield return entry.GetCurrent();
			}
		}
	}

	private void Awake()
	{
		SetIsPaused(false);
	}

	private void OnApplicationPause(bool paused)
	{
		SetIsPaused(paused);
	}
}
