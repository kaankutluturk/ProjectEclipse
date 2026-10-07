using System;

public sealed class ObjectNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory _objectFactory;

	private readonly ITypeInspector _typeDescriptor;

	private readonly bool _ignoreUnmatched;

	public ObjectNodeDeserializer(IObjectFactory factory, ITypeInspector typeDescriptor, bool ignoreUnmatched)
	{
		_objectFactory = factory;
		_typeDescriptor = typeDescriptor;
		_ignoreUnmatched = ignoreUnmatched;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		MappingStart mappingStart = reader.Allow<MappingStart>();
		if (mappingStart == null)
		{
			value = null;
			return false;
		}
		value = _objectFactory.Create(expectedType);
		while (!reader.Accept<MappingEnd>())
		{
			Scalar scalar = reader.Expect<Scalar>();
			IPropertyDescriptor property = _typeDescriptor.GetProperty(expectedType, null, scalar.GetValue(), _ignoreUnmatched);
			if (property == null)
			{
				reader.SkipThisAndNestedEvents();
				continue;
			}
			object obj = nestedObjectDeserializer(reader, property.get_Type());
			IValuePromise valuePromise = obj as IValuePromise;
			if (valuePromise == null)
			{
				object convertedValue = TypeConverterHelper.ChangeType(obj, property.get_Type());
				property.Write(value, convertedValue);
				continue;
			}
			object valueRef = value;
			valuePromise.add_ValueAvailable((object resolvedValue) =>
			{
				object bAINMLLIKOL2 = TypeConverterHelper.ChangeType(resolvedValue, property.get_Type());
				property.Write(valueRef, bAINMLLIKOL2);
			});
		}
		reader.Expect<MappingEnd>();
		return true;
	}
}
