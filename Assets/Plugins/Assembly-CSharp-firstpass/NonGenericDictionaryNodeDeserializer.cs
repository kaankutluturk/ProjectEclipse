using System;
using System.Collections;

public sealed class NonGenericDictionaryNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public NonGenericDictionaryNodeDeserializer(IObjectFactory EJPHFDCKCCE)
	{
		objectFactory = EJPHFDCKCCE;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		if (!typeof(IDictionary).IsAssignableFrom(MBLGNMBFHBI))
		{
			value = false;
			return false;
		}
		reader.Expect<MappingStart>();
		IDictionary dictionary = (IDictionary)objectFactory.Create(MBLGNMBFHBI);
		while (!reader.Accept<MappingEnd>())
		{
			object KGBGENDIMBC = IJBAEAEDMCC(reader, typeof(object));
			IValuePromise aGAMFLELGLG = KGBGENDIMBC as IValuePromise;
			object EJMKBJGNOOB = IJBAEAEDMCC(reader, typeof(object));
			IValuePromise aGAMFLELGLG2 = EJMKBJGNOOB as IValuePromise;
			if (aGAMFLELGLG == null)
			{
				if (aGAMFLELGLG2 == null)
				{
					dictionary.Add(KGBGENDIMBC, EJMKBJGNOOB);
					continue;
				}
				aGAMFLELGLG2.add_ValueAvailable((object AFIEJABPAKA) =>
				{
					dictionary.Add(KGBGENDIMBC, AFIEJABPAKA);
				});
				continue;
			}
			if (aGAMFLELGLG2 == null)
			{
				aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
				{
					dictionary.Add(AFIEJABPAKA, EJMKBJGNOOB);
				});
				continue;
			}
			bool hasFirstPart = false;
			aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
			{
				if (hasFirstPart)
				{
					dictionary.Add(AFIEJABPAKA, EJMKBJGNOOB);
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
					dictionary.Add(KGBGENDIMBC, AFIEJABPAKA);
				}
				else
				{
					EJMKBJGNOOB = AFIEJABPAKA;
					hasFirstPart = true;
				}
			});
		}
		value = dictionary;
		reader.Expect<MappingEnd>();
		return true;
	}
}
