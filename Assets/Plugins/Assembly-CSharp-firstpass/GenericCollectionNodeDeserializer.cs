using System;
using System.Collections.Generic;
using YamlDotNet.Core;

public sealed class GenericCollectionNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private static readonly GenericStaticMethod deserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object>(null, null, null, null));

	public GenericCollectionNodeDeserializer(IObjectFactory factory)
	{
		objectFactory = factory;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		Type type = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(ICollection<>));
		if (type == null)
		{
			value = false;
			return false;
		}
		value = objectFactory.Create(expectedType);
		deserializeHelperMethod.Invoke(type.GetGenericArguments(), reader, expectedType, nestedObjectDeserializer, value);
		return true;
	}

	internal static void DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, ICollection<TItem> collection)
	{
		IList<TItem> list = collection as IList<TItem>;
		reader.Expect<SequenceStart>();
		while (!reader.Accept<SequenceEnd>())
		{
			ParsingEvent currentEvent = reader.GetParser().GetCurrent();
			object obj = nestedObjectDeserializer(reader, typeof(TItem));
			IValuePromise valuePromise = obj as IValuePromise;
			if (valuePromise == null)
			{
				collection.Add(TypeConverterHelper.ChangeType<TItem>(obj));
				continue;
			}
			if (list != null)
			{
				int index = list.Count;
				collection.Add(default(TItem));
				valuePromise.add_ValueAvailable((object resolved) =>
				{
					list[index] = TypeConverterHelper.ChangeType<TItem>(resolved);
				});
				continue;
			}
			throw new ForwardAnchorNotSupportedException(currentEvent.GetStart(), currentEvent.GetEnd(), "Forward alias references are not allowed because this type does not implement IList<>");
		}
		reader.Expect<SequenceEnd>();
	}
}
