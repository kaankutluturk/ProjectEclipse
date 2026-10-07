using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SerializerState : IDisposable
{
	private readonly IDictionary<Type, object> items = new Dictionary<Type, object>();

	public T Get<T>() where T : class, new()
	{
		object value;
		if (!items.TryGetValue(typeof(T), out value))
		{
			value = new T();
			items.Add(typeof(T), value);
		}
		return (T)value;
	}

	public void OnDeserialization()
	{
		foreach (IPostDeserializationCallback item in items.Values.OfType<IPostDeserializationCallback>())
		{
			item.OnDeserializationComplete();
		}
	}

	public void Dispose()
	{
		foreach (IDisposable item in items.Values.OfType<IDisposable>())
		{
			item.Dispose();
		}
	}
}
