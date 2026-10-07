using System;

public interface IValuePromise
{
	event Action<object> ValueAvailableEvent;

	void add_ValueAvailable(Action<object> value);

	void remove_ValueAvailable(Action<object> value);
}
