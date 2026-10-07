using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ExtentionBehaviour : MonoBehaviour
{
	public class CallEventArgs
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private object target;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int eventId;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private object content;

		public int ExtEventId
		{
			get
			{
				return GetEventId();
			}
			private set
			{
				set_Event(value);
			}
		}

		public object Content
		{
			get
			{
				return GetContent();
			}
			private set
			{
				SetContent(value);
			}
		}

		public CallEventArgs(int IILOLJJLLGH, object DMNBDBJNKME, object target)
		{
			set_Target(target);
			SetContent(DMNBDBJNKME);
			set_Event(IILOLJJLLGH);
		}

		public object GetTarget()
		{
			return target;
		}

		private void set_Target(object value)
		{
			target = value;
		}

		public int GetEventId()
		{
			return eventId;
		}

		private void set_Event(int value)
		{
			eventId = value;
		}

		public object GetContent()
		{
			return content;
		}

		private void SetContent(object value)
		{
			content = value;
		}

		public CallEventArgs SwitchTarget(object target)
		{
			set_Target(target);
			return this;
		}
	}

	public enum BehaviourEventType
	{
		SimpeEvent = 0
	}

	private GameObject _gameObject;

	private Transform _transform;

	private Renderer _renderer;

	private Animator _animator;

	private readonly Dictionary<int, List<Action<CallEventArgs>>> eventListeners = new Dictionary<int, List<Action<CallEventArgs>>>();

	public GameObject CachedGameObject
	{
		get
		{
			return get_gameObject();
		}
	}

	public Transform CachedTransform
	{
		get
		{
			return get_transform();
		}
	}

	public Renderer CachedRenderer
	{
		get
		{
			return get_renderer();
		}
	}

	public Animator CachedAnimator
	{
		get
		{
			return get_animator();
		}
	}

	protected void Log(object LIOGIBJBHAH, UnityEngine.Object BBNKIBKPBLO = null)
	{
		AdvLog.Log(LIOGIBJBHAH, BBNKIBKPBLO ?? this);
	}

	protected void LogWarning(object LIOGIBJBHAH, UnityEngine.Object BBNKIBKPBLO = null)
	{
		AdvLog.LogWarning(LIOGIBJBHAH, BBNKIBKPBLO ?? this);
	}

	protected void LogError(object LIOGIBJBHAH, UnityEngine.Object BBNKIBKPBLO = null)
	{
		AdvLog.LogError(LIOGIBJBHAH, BBNKIBKPBLO ?? this);
	}

	protected void LogException(Exception MPFFFAOGBJE, UnityEngine.Object BBNKIBKPBLO = null)
	{
		AdvLog.LogException(MPFFFAOGBJE, BBNKIBKPBLO ?? this);
	}

	public GameObject get_gameObject()
	{
		if (!_gameObject)
		{
			_gameObject = base.gameObject;
		}
		return _gameObject;
	}

	public Transform get_transform()
	{
		if (!_transform)
		{
			_transform = base.transform;
		}
		return _transform;
	}

	public Renderer get_renderer()
	{
		if (!_renderer)
		{
			_renderer = GetComponent<Renderer>();
		}
		return _renderer;
	}

	public Animator get_animator()
	{
		if (!_animator)
		{
			_animator = GetComponent<Animator>();
		}
		return _animator;
	}

	protected void Invoke(Action IBODMPMJELJ, float GNAONAPDDLD)
	{
		StartCoroutine(InvokeActionRoutine(IBODMPMJELJ, GNAONAPDDLD));
	}

	private IEnumerator InvokeActionRoutine(Action IBODMPMJELJ, float GNAONAPDDLD)
	{
		yield return new WaitForSeconds(GNAONAPDDLD);
		IBODMPMJELJ();
	}

	public void addEventListener(int IILOLJJLLGH, Action<CallEventArgs> callback)
	{
		if (!eventListeners.ContainsKey(IILOLJJLLGH))
		{
			eventListeners.Add(IILOLJJLLGH, new List<Action<CallEventArgs>>());
		}
		eventListeners[IILOLJJLLGH].Add(callback);
	}

	public void addEventListener(int[] IILOLJJLLGH, Action<CallEventArgs> callback)
	{
		for (int i = 0; i < IILOLJJLLGH.Length; i++)
		{
			addEventListener(IILOLJJLLGH[i], callback);
		}
	}

	private void RemoveAllEventListeners()
	{
		eventListeners.Clear();
	}

	public void removeEvent(int IILOLJJLLGH)
	{
		if (eventListeners.ContainsKey(IILOLJJLLGH))
		{
			eventListeners.Remove(IILOLJJLLGH);
		}
	}

	public void removeEventListener(int IILOLJJLLGH, Action<CallEventArgs> callback)
	{
		if (eventListeners.ContainsKey(IILOLJJLLGH))
		{
			while (eventListeners[IILOLJJLLGH].Contains(callback))
			{
				eventListeners[IILOLJJLLGH].Remove(callback);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		RemoveAllEventListeners();
	}

	public void callEvent(int IILOLJJLLGH, object DMNBDBJNKME = null)
	{
		if (!eventListeners.ContainsKey(IILOLJJLLGH))
		{
			return;
		}
		List<Action<CallEventArgs>> list = new List<Action<CallEventArgs>>(eventListeners[IILOLJJLLGH]);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && eventListeners.ContainsKey(IILOLJJLLGH) && eventListeners[IILOLJJLLGH].Contains(list[i]))
			{
				list[i](new CallEventArgs(IILOLJJLLGH, DMNBDBJNKME, this));
			}
		}
	}
}
