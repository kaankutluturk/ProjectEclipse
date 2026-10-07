using System;
using System.Collections;

public sealed class NonGenericListNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public NonGenericListNodeDeserializer(IObjectFactory factory)
	{
		objectFactory = factory;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		if (!typeof(IList).IsAssignableFrom(expectedType))
		{
			value = false;
			return false;
		}
		reader.Expect<SequenceStart>();
		IList list = (IList)objectFactory.Create(expectedType);
		while (!reader.Accept<SequenceEnd>())
		{
			object obj = nestedObjectDeserializer(reader, typeof(object));
			IValuePromise valuePromise = obj as IValuePromise;
			if (valuePromise == null)
			{
				list.Add(obj);
				continue;
			}
			int index = list.Count;
			list.Add(null);
			valuePromise.add_ValueAvailable((object resolvedValue) =>
			{
				list[index] = resolvedValue;
			});
		}
		value = list;
		reader.Expect<SequenceEnd>();
		return true;
	}
}
