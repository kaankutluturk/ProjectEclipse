using System;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	public class SFMonoBehaviour<T> : MonoBehaviour, global::IEventDispatcher<T>
	{
		private global::EventDispatcher<T> eventDispatcher = new global::EventDispatcher<T>();

		public int AddEventListener(int name, Action<T> ODDEOFKLIAG)
		{
			return eventDispatcher.AddEventListener(name, ODDEOFKLIAG);
		}

		public int CallEvent(int name, T EHCLMBADLKH)
		{
			return eventDispatcher.CallEvent(name, EHCLMBADLKH);
		}

		public int RemoveAllEventListener()
		{
			return eventDispatcher.RemoveAllEventListener();
		}

		public int RemoveEvent(int name)
		{
			return eventDispatcher.RemoveEvent(name);
		}

		public int RemoveEventListener(int name, Action<T> ODDEOFKLIAG)
		{
			return eventDispatcher.RemoveEventListener(name, ODDEOFKLIAG);
		}
	}
}
