using System;

public sealed class StaticTypeResolver : ITypeResolver
{
	public Type Resolve(Type type, object value)
	{
		return type;
	}
}
