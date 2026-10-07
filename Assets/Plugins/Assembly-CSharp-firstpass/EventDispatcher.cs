using System;
using System.Collections.Generic;

public class EventDispatcher<T> : global::IEventDispatcher<T>
{
	private Dictionary<int, Action<T>> listeners;

	public EventDispatcher()
	{
		listeners = new Dictionary<int, Action<T>>();
	}

	public int AddEventListener(int name, Action<T> ODDEOFKLIAG)
	{
		if (ODDEOFKLIAG == null)
		{
			return -1;
		}
		if (listeners.ContainsKey(name))
		{
			Dictionary<int, Action<T>> aPNNBCCKAJA;
			int key;
			(aPNNBCCKAJA = listeners)[key = name] = (Action<T>)Delegate.Combine(aPNNBCCKAJA[key], ODDEOFKLIAG);
			return 0;
		}
		listeners.Add(name, ODDEOFKLIAG);
		return 1;
	}

	public int RemoveEventListener(int name, Action<T> ODDEOFKLIAG)
	{
		if (ODDEOFKLIAG == null)
		{
			return -1;
		}
		if (listeners.ContainsKey(name))
		{
			Dictionary<int, Action<T>> aPNNBCCKAJA;
			int key;
			(aPNNBCCKAJA = listeners)[key = name] = (Action<T>)Delegate.Remove(aPNNBCCKAJA[key], ODDEOFKLIAG);
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

	public int CallEvent(int name, T EHCLMBADLKH)
	{
		if (listeners.ContainsKey(name))
		{
			listeners[name](EHCLMBADLKH);
			return 0;
		}
		return 1;
	}
}
