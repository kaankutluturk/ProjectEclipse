using System;
using System.Collections;
using System.Collections.Generic;

public sealed class EnumerableNodeDeserializer : INodeDeserializer
{
	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		Type type;
		if (expectedType == typeof(IEnumerable))
		{
			type = typeof(object);
		}
		else
		{
			Type type2 = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IEnumerable<>));
			if (type2 != expectedType)
			{
				value = null;
				return false;
			}
			type = type2.GetGenericArguments()[0];
		}
		Type arg = typeof(List<>).MakeGenericType(type);
		value = nestedObjectDeserializer(reader, arg);
		return true;
	}
}
