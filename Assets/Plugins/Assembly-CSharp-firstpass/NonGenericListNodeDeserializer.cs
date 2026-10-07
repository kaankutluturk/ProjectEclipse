using System;
using System.Collections;

public sealed class NonGenericListNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public NonGenericListNodeDeserializer(IObjectFactory EJPHFDCKCCE)
	{
		objectFactory = EJPHFDCKCCE;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		if (!typeof(IList).IsAssignableFrom(MBLGNMBFHBI))
		{
			value = false;
			return false;
		}
		reader.Expect<SequenceStart>();
		IList GBAAEMCBDAM = (IList)objectFactory.Create(MBLGNMBFHBI);
		while (!reader.Accept<SequenceEnd>())
		{
			object obj = IJBAEAEDMCC(reader, typeof(object));
			IValuePromise aGAMFLELGLG = obj as IValuePromise;
			if (aGAMFLELGLG == null)
			{
				GBAAEMCBDAM.Add(obj);
				continue;
			}
			int index = GBAAEMCBDAM.Count;
			GBAAEMCBDAM.Add(null);
			aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
			{
				GBAAEMCBDAM[index] = AFIEJABPAKA;
			});
		}
		value = GBAAEMCBDAM;
		reader.Expect<SequenceEnd>();
		return true;
	}
}
