using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TypeConverterNodeDeserializer : INodeDeserializer
{
	private readonly IEnumerable<IYamlTypeConverter> converters;

	public TypeConverterNodeDeserializer(IEnumerable<IYamlTypeConverter> typeConverters)
	{
		if (typeConverters == null)
		{
			throw new ArgumentNullException("converters");
		}
		this.converters = typeConverters;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type type, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		IYamlTypeConverter matchingConverter = converters.FirstOrDefault((IYamlTypeConverter converter) => converter.Accepts(type));
		if (matchingConverter == null)
		{
			value = null;
			return false;
		}
		value = matchingConverter.ReadYaml(reader.GetParser(), type);
		return true;
	}
}
