using System;

public interface IEventDispatcher<T>
{
	int AddEventListener(int name, Action<T> listener);

	int RemoveEventListener(int name, Action<T> listener);

	int RemoveAllEventListener();

	int RemoveEvent(int name);

	int CallEvent(int name, T eventArgs);
}
