using System;
using System.Collections.Generic;

public sealed class GenericDictionaryNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private static readonly GenericStaticMethod deserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object, object>(null, null, null, null));

	public GenericDictionaryNodeDeserializer(IObjectFactory EJPHFDCKCCE)
	{
		objectFactory = EJPHFDCKCCE;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		Type type = ReflectionUtility.GetImplementedGenericInterface(MBLGNMBFHBI, typeof(IDictionary<, >));
		if (type == null)
		{
			value = false;
			return false;
		}
		reader.Expect<MappingStart>();
		value = objectFactory.Create(MBLGNMBFHBI);
		deserializeHelperMethod.Invoke(type.GetGenericArguments(), reader, MBLGNMBFHBI, IJBAEAEDMCC, value);
		reader.Expect<MappingEnd>();
		return true;
	}

	private static void DeserializeHelper<TKey, TValue>(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, IDictionary<TKey, TValue> DCJLKCFKCOM)
	{
		while (!reader.Accept<MappingEnd>())
		{
			object KGBGENDIMBC = IJBAEAEDMCC(reader, typeof(TKey));
			IValuePromise aGAMFLELGLG = KGBGENDIMBC as IValuePromise;
			object value = IJBAEAEDMCC(reader, typeof(TValue));
			IValuePromise aGAMFLELGLG2 = value as IValuePromise;
			if (aGAMFLELGLG == null)
			{
				if (aGAMFLELGLG2 == null)
				{
					DCJLKCFKCOM[(TKey)KGBGENDIMBC] = (TValue)value;
					continue;
				}
				aGAMFLELGLG2.add_ValueAvailable((object AFIEJABPAKA) =>
				{
					DCJLKCFKCOM[(TKey)KGBGENDIMBC] = (TValue)AFIEJABPAKA;
				});
				continue;
			}
			if (aGAMFLELGLG2 == null)
			{
				aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
				{
					DCJLKCFKCOM[(TKey)AFIEJABPAKA] = (TValue)value;
				});
				continue;
			}
			bool hasFirstPart = false;
			aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
			{
				if (hasFirstPart)
				{
					DCJLKCFKCOM[(TKey)AFIEJABPAKA] = (TValue)value;
				}
				else
				{
					KGBGENDIMBC = AFIEJABPAKA;
					hasFirstPart = true;
				}
			});
			aGAMFLELGLG2.add_ValueAvailable((object AFIEJABPAKA) =>
			{
				if (hasFirstPart)
				{
					DCJLKCFKCOM[(TKey)KGBGENDIMBC] = (TValue)AFIEJABPAKA;
				}
				else
				{
					value = AFIEJABPAKA;
					hasFirstPart = true;
				}
			});
		}
	}
}
