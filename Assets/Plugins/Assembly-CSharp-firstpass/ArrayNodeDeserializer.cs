using System;
using System.Collections.Generic;

public sealed class ArrayNodeDeserializer : INodeDeserializer
{
	private static readonly GenericStaticMethod DeserializeHelperMethod = new GenericStaticMethod(() => DeserializeHelper<object>(null, null, null));

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		if (!MBLGNMBFHBI.IsArray)
		{
			value = false;
			return false;
		}
		value = DeserializeHelperMethod.Invoke(new Type[1] { MBLGNMBFHBI.GetElementType() }, reader, MBLGNMBFHBI, IJBAEAEDMCC);
		return true;
	}

	private static TItem[] DeserializeHelper<TItem>(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC)
	{
		List<TItem> list = new List<TItem>();
		GenericCollectionNodeDeserializer.DeserializeHelper(reader, MBLGNMBFHBI, IJBAEAEDMCC, list);
		return list.ToArray();
	}
}
