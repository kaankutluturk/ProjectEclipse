using System;

public sealed class DynamicTypeResolver : ITypeResolver
{
	public Type Resolve(Type FGDJAEMHFKC, object LNDKPCCDCOB)
	{
		return (LNDKPCCDCOB == null) ? FGDJAEMHFKC : LNDKPCCDCOB.GetType();
	}
}
