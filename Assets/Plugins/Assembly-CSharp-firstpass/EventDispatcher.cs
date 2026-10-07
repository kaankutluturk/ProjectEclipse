using System;
using System.Collections.Generic;

public class EventDispatcher<T> : global::IEventDispatcher<T>
{
	private Dictionary<int, Action<T>> listeners;

	public EventDispatcher()
	{
		listeners = new Dictionary<int, Action<T>>();
	}

	public int AddEventListener(int name, Action<T> listener)
	{
		if (listener == null)
		{
			return -1;
		}
		if (listeners.ContainsKey(name))
		{
			Dictionary<int, Action<T>> table;
			int key;
			(table = listeners)[key = name] = (Action<T>)Delegate.Combine(table[key], listener);
			return 0;
		}
		listeners.Add(name, listener);
		return 1;
	}

	public int RemoveEventListener(int name, Action<T> listener)
	{
		if (listener == null)
		{
			return -1;
		}
		if (listeners.ContainsKey(name))
		{
			Dictionary<int, Action<T>> table;
			int key;
			(table = listeners)[key = name] = (Action<T>)Delegate.Remove(table[key], listener);
			if (listeners[name] == null)
			{
				RemoveEvent(name);
			}
			return 0;
		}
		return 1;
	}

	public int RemoveAllEventListener()
	{
		listeners.Clear();
		return 0;
	}

	public int RemoveEvent(int name)
	{
		listeners.Remove(name);
		return 1;
	}

	public int CallEvent(int name, T eventArgs)
	{
		if (listeners.ContainsKey(name))
		{
			listeners[name](eventArgs);
			return 0;
		}
		return 1;
	}
}
