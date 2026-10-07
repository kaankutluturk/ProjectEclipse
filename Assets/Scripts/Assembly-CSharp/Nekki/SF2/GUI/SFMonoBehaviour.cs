using System;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	public class SFMonoBehaviour<T> : MonoBehaviour, global::IEventDispatcher<T>
	{
		private global::EventDispatcher<T> eventDispatcher = new global::EventDispatcher<T>();

		public int AddEventListener(int name, Action<T> callback)
		{
			return eventDispatcher.AddEventListener(name, callback);
		}

		public int CallEvent(int name, T data)
		{
			return eventDispatcher.CallEvent(name, data);
		}

		public int RemoveAllEventListener()
		{
			return eventDispatcher.RemoveAllEventListener();
		}

		public int RemoveEvent(int name)
		{
			return eventDispatcher.RemoveEvent(name);
		}

		public int RemoveEventListener(int name, Action<T> callback)
		{
			return eventDispatcher.RemoveEventListener(name, callback);
		}
	}
}
