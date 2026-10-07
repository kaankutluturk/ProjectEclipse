using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TypeConverterNodeDeserializer : INodeDeserializer
{
	private readonly IEnumerable<IYamlTypeConverter> converters;

	public TypeConverterNodeDeserializer(IEnumerable<IYamlTypeConverter> JNONHBMNKDK)
	{
		if (JNONHBMNKDK == null)
		{
			throw new ArgumentNullException("converters");
		}
		this.converters = JNONHBMNKDK;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		IYamlTypeConverter bLNPLLKJFLC = converters.FirstOrDefault((IYamlTypeConverter ILHDJDNPFKH) => ILHDJDNPFKH.Accepts(MBLGNMBFHBI));
		if (bLNPLLKJFLC == null)
		{
			value = null;
			return false;
		}
		value = bLNPLLKJFLC.ReadYaml(reader.GetParser(), MBLGNMBFHBI);
		return true;
	}
}
