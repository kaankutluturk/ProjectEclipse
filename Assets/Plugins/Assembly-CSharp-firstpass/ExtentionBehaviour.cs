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

		public CallEventArgs(int eventId, object content, object target)
		{
			set_Target(target);
			SetContent(content);
			set_Event(eventId);
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

	protected void Log(object message, UnityEngine.Object context = null)
	{
		AdvLog.Log(message, context ?? this);
	}

	protected void LogWarning(object message, UnityEngine.Object context = null)
	{
		AdvLog.LogWarning(message, context ?? this);
	}

	protected void LogError(object message, UnityEngine.Object context = null)
	{
		AdvLog.LogError(message, context ?? this);
	}

	protected void LogException(Exception exception, UnityEngine.Object context = null)
	{
		AdvLog.LogException(exception, context ?? this);
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

	protected void Invoke(Action action, float delay)
	{
		StartCoroutine(InvokeActionRoutine(action, delay));
	}

	private IEnumerator InvokeActionRoutine(Action action, float delay)
	{
		yield return new WaitForSeconds(delay);
		action();
	}

	public void addEventListener(int eventId, Action<CallEventArgs> callback)
	{
		if (!eventListeners.ContainsKey(eventId))
		{
			eventListeners.Add(eventId, new List<Action<CallEventArgs>>());
		}
		eventListeners[eventId].Add(callback);
	}

	public void addEventListener(int[] eventIds, Action<CallEventArgs> callback)
	{
		for (int i = 0; i < eventIds.Length; i++)
		{
			addEventListener(eventIds[i], callback);
		}
	}

	private void RemoveAllEventListeners()
	{
		eventListeners.Clear();
	}

	public void removeEvent(int eventId)
	{
		if (eventListeners.ContainsKey(eventId))
		{
			eventListeners.Remove(eventId);
		}
	}

	public void removeEventListener(int eventId, Action<CallEventArgs> callback)
	{
		if (eventListeners.ContainsKey(eventId))
		{
			while (eventListeners[eventId].Contains(callback))
			{
				eventListeners[eventId].Remove(callback);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		RemoveAllEventListeners();
	}

	public void callEvent(int eventId, object content = null)
	{
		if (!eventListeners.ContainsKey(eventId))
		{
			return;
		}
		List<Action<CallEventArgs>> list = new List<Action<CallEventArgs>>(eventListeners[eventId]);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && eventListeners.ContainsKey(eventId) && eventListeners[eventId].Contains(list[i]))
			{
				list[i](new CallEventArgs(eventId, content, this));
			}
		}
	}
}
