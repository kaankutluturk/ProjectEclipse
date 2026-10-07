using System;
using System.Collections;

public sealed class NonGenericDictionaryNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public NonGenericDictionaryNodeDeserializer(IObjectFactory factory)
	{
		objectFactory = factory;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		if (!typeof(IDictionary).IsAssignableFrom(expectedType))
		{
			value = false;
			return false;
		}
		reader.Expect<MappingStart>();
		IDictionary dictionary = (IDictionary)objectFactory.Create(expectedType);
		while (!reader.Accept<MappingEnd>())
		{
			object key = nestedObjectDeserializer(reader, typeof(object));
			IValuePromise keyPromise = key as IValuePromise;
			object entryValue = nestedObjectDeserializer(reader, typeof(object));
			IValuePromise aGAMFLELGLG2 = entryValue as IValuePromise;
			if (keyPromise == null)
			{
				if (aGAMFLELGLG2 == null)
				{
					dictionary.Add(key, entryValue);
					continue;
				}
				aGAMFLELGLG2.add_ValueAvailable((object resolvedValue) =>
				{
					dictionary.Add(key, resolvedValue);
				});
				continue;
			}
			if (aGAMFLELGLG2 == null)
			{
				keyPromise.add_ValueAvailable((object resolvedValue) =>
				{
					dictionary.Add(resolvedValue, entryValue);
				});
				continue;
			}
			bool hasFirstPart = false;
			keyPromise.add_ValueAvailable((object resolvedValue) =>
			{
				if (hasFirstPart)
				{
					dictionary.Add(resolvedValue, entryValue);
				}
				else
				{
					key = resolvedValue;
					hasFirstPart = true;
				}
			});
			aGAMFLELGLG2.add_ValueAvailable((object resolvedValue) =>
			{
				if (hasFirstPart)
				{
					dictionary.Add(key, resolvedValue);
				}
				else
				{
					entryValue = resolvedValue;
					hasFirstPart = true;
				}
			});
		}
		value = dictionary;
		reader.Expect<MappingEnd>();
		return true;
	}
}
