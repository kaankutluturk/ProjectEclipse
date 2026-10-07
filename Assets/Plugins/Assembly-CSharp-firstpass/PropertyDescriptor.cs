using System;
using System.Diagnostics;

public sealed class PropertyDescriptor : IPropertyDescriptor
{
	private readonly IPropertyDescriptor baseDescriptor;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int order;

	public string PropertyName
	{
		get
		{
			return get_Name();
		}
		set
		{
			set_Name(value);
		}
	}

	public Type OverrideType
	{
		get
		{
			return GetTypeOverride();
		}
		set
		{
			set_TypeOverride(value);
		}
	}

	public int SortOrder
	{
		get
		{
			return GetOrder();
		}
		set
		{
			set_Order(value);
		}
	}

	public bool CanWrite
	{
		get
		{
			return GetCanWrite();
		}
	}

	public PropertyDescriptor(IPropertyDescriptor PNBMNIMMEOF)
	{
		this.baseDescriptor = PNBMNIMMEOF;
		set_Name(PNBMNIMMEOF.get_Name());
	}

	public string get_Name()
	{
		return name;
	}

	public void set_Name(string value)
	{
		name = value;
	}

	public Type get_Type()
	{
		return baseDescriptor.get_Type();
	}

	public Type GetTypeOverride()
	{
		return baseDescriptor.GetTypeOverride();
	}

	public void set_TypeOverride(Type value)
	{
		baseDescriptor.set_TypeOverride(value);
	}

	public int GetOrder()
	{
		return order;
	}

	public void set_Order(int value)
	{
		order = value;
	}

	public bool GetCanWrite()
	{
		return baseDescriptor.GetCanWrite();
	}

	public void Write(object target, object value)
	{
		baseDescriptor.Write(target, value);
	}

	public T GetCustomAttribute<T>() where T : Attribute
	{
		return baseDescriptor.GetCustomAttribute<T>();
	}

	public IObjectDescriptor Read(object target)
	{
		return baseDescriptor.Read(target);
	}
}
