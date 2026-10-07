using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ReadableAndWritablePropertiesTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	public ReadableAndWritablePropertiesTypeInspector(ITypeInspector innerInspector)
	{
		innerTypeDescriptor = innerInspector;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
	{
		return from property in innerTypeDescriptor.GetProperties(type, container)
			where property.GetCanWrite()
			select property;
	}
}
