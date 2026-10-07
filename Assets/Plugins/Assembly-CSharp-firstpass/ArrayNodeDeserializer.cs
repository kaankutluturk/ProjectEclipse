using System;
using System.Collections.Generic;

public sealed class ArrayNodeDeserializer : INodeDeserializer
{
	private static readonly GenericStaticMethod DeserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object>(null, null, null));

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		if (!expectedType.IsArray)
		{
			value = false;
			return false;
		}
		value = DeserializeHelperMethod.Invoke(new Type[1] { expectedType.GetElementType() }, reader, expectedType, nestedObjectDeserializer);
		return true;
	}

	private static TItem[] DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer)
	{
		List<TItem> list = new List<TItem>();
		GenericCollectionNodeDeserializer.DeserializeHelper(reader, expectedType, nestedObjectDeserializer, list);
		return list.ToArray();
	}
}
