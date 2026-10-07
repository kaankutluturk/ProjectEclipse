using System;
using System.Collections.Generic;
using YamlDotNet.Core;

public sealed class GenericCollectionNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private static readonly GenericStaticMethod deserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object>(null, null, null, null));

	public GenericCollectionNodeDeserializer(IObjectFactory EJPHFDCKCCE)
	{
		objectFactory = EJPHFDCKCCE;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		Type type = ReflectionUtility.GetImplementedGenericInterface(MBLGNMBFHBI, typeof(ICollection<>));
		if (type == null)
		{
			value = false;
			return false;
		}
		value = objectFactory.Create(MBLGNMBFHBI);
		deserializeHelperMethod.Invoke(type.GetGenericArguments(), reader, MBLGNMBFHBI, IJBAEAEDMCC, value);
		return true;
	}

	internal static void DeserializeHelper<TItem>(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, ICollection<TItem> DCJLKCFKCOM)
	{
		IList<TItem> GBAAEMCBDAM = DCJLKCFKCOM as IList<TItem>;
		reader.Expect<SequenceStart>();
		while (!reader.Accept<SequenceEnd>())
		{
			ParsingEvent jMKLCDAKEOG = reader.GetParser().GetCurrent();
			object obj = IJBAEAEDMCC(reader, typeof(TItem));
			IValuePromise aGAMFLELGLG = obj as IValuePromise;
			if (aGAMFLELGLG == null)
			{
				DCJLKCFKCOM.Add(TypeConverterHelper.ChangeType<TItem>(obj));
				continue;
			}
			if (GBAAEMCBDAM != null)
			{
				int index = GBAAEMCBDAM.Count;
				DCJLKCFKCOM.Add(default(TItem));
				aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
				{
					GBAAEMCBDAM[index] = TypeConverterHelper.ChangeType<TItem>(AFIEJABPAKA);
				});
				continue;
			}
			throw new ForwardAnchorNotSupportedException(jMKLCDAKEOG.GetStart(), jMKLCDAKEOG.GetEnd(), "Forward alias references are not allowed because this type does not implement IList<>");
		}
		reader.Expect<SequenceEnd>();
	}
}
