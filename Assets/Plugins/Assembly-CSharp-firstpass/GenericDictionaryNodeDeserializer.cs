using System;
using System.Collections.Generic;

public sealed class GenericDictionaryNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private static readonly GenericStaticMethod deserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object, object>(null, null, null, null));

	public GenericDictionaryNodeDeserializer(IObjectFactory factory)
	{
		objectFactory = factory;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		Type type = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<, >));
		if (type == null)
		{
			value = false;
			return false;
		}
		reader.Expect<MappingStart>();
		value = objectFactory.Create(expectedType);
		deserializeHelperMethod.Invoke(type.GetGenericArguments(), reader, expectedType, nestedObjectDeserializer, value);
		reader.Expect<MappingEnd>();
		return true;
	}

	private static void DeserializeHelper<TKey, TValue>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, IDictionary<TKey, TValue> dictionary)
	{
		while (!reader.Accept<MappingEnd>())
		{
			object key = nestedObjectDeserializer(reader, typeof(TKey));
			IValuePromise keyPromise = key as IValuePromise;
			object value = nestedObjectDeserializer(reader, typeof(TValue));
			IValuePromise aGAMFLELGLG2 = value as IValuePromise;
			if (keyPromise == null)
			{
				if (aGAMFLELGLG2 == null)
				{
					dictionary[(TKey)key] = (TValue)value;
					continue;
				}
				aGAMFLELGLG2.add_ValueAvailable((object resolved) =>
				{
					dictionary[(TKey)key] = (TValue)resolved;
				});
				continue;
			}
			if (aGAMFLELGLG2 == null)
			{
				keyPromise.add_ValueAvailable((object resolved) =>
				{
					dictionary[(TKey)resolved] = (TValue)value;
				});
				continue;
			}
			bool hasFirstPart = false;
			keyPromise.add_ValueAvailable((object resolved) =>
			{
				if (hasFirstPart)
				{
					dictionary[(TKey)resolved] = (TValue)value;
				}
				else
				{
					key = resolved;
					hasFirstPart = true;
				}
			});
			aGAMFLELGLG2.add_ValueAvailable((object resolved) =>
			{
				if (hasFirstPart)
				{
					dictionary[(TKey)key] = (TValue)resolved;
				}
				else
				{
					value = resolved;
					hasFirstPart = true;
				}
			});
		}
	}
}
