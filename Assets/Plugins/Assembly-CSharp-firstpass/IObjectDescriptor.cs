using System;

public interface IObjectDescriptor
{
	object Value { get; }

	Type StaticType { get; }

	object GetValue();

	Type get_Type();

	Type GetStaticType();
}
