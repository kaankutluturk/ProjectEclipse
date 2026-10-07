using System;
using System.Diagnostics;

public sealed class ObjectDescriptor : IObjectDescriptor
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object wrappedValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Type type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Type staticType;

	public Type StaticType
	{
		get
		{
			return GetStaticType();
		}
		private set
		{
			SetStaticType(value);
		}
	}

	public ObjectDescriptor(object value, Type LFLGCDNKNJI, Type FGDJAEMHFKC)
	{
		set_Value(value);
		if (LFLGCDNKNJI == null)
		{
			throw new ArgumentNullException("type");
		}
		set_Type(LFLGCDNKNJI);
		if (FGDJAEMHFKC == null)
		{
			throw new ArgumentNullException("staticType");
		}
		SetStaticType(FGDJAEMHFKC);
	}

	object IObjectDescriptor.Value
	{
		get
		{
			return GetValue();
		}
	}

	public object GetValue()
	{
		return wrappedValue;
	}

	private void set_Value(object value)
	{
		wrappedValue = value;
	}

	public Type get_Type()
	{
		return type;
	}

	private void set_Type(Type value)
	{
		type = value;
	}

	public Type GetStaticType()
	{
		return staticType;
	}

	private void SetStaticType(Type value)
	{
		staticType = value;
	}
}
