using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class YamlMemberAttribute : Attribute
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Type serializeAs;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int order;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string memberName;

	public Type SerializeAsType
	{
		get
		{
			return GetSerializeAs();
		}
		set
		{
			set_SerializeAs(value);
		}
	}

	public int OrderIndex
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

	public string MemberName
	{
		get
		{
			return GetName();
		}
		set
		{
			set_Alias(value);
		}
	}

	public YamlMemberAttribute()
	{
	}

	public YamlMemberAttribute(Type JDBOFNJPMPH)
	{
		set_SerializeAs(JDBOFNJPMPH);
	}

	public Type GetSerializeAs()
	{
		return serializeAs;
	}

	public void set_SerializeAs(Type value)
	{
		serializeAs = value;
	}

	public int GetOrder()
	{
		return order;
	}

	public void set_Order(int value)
	{
		order = value;
	}

	public string GetName()
	{
		return memberName;
	}

	public void set_Alias(string value)
	{
		memberName = value;
	}
}
