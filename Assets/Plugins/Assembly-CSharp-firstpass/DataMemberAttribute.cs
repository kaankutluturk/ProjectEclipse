using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DataMemberAttribute : Attribute
{
	private bool isRequired;

	private bool emitDefaultValue = true;

	private string name;

	private int order = -1;

	public bool EmitDefaultValue
	{
		get
		{
			return GetEmitDefaultValue();
		}
		set
		{
			SetEmitDefaultValue(value);
		}
	}

	public bool IsRequired
	{
		get
		{
			return GetIsRequired();
		}
		set
		{
			SetIsRequired(value);
		}
	}

	public string MemberName
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

	public int SerializationOrder
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

	public bool GetEmitDefaultValue()
	{
		return emitDefaultValue;
	}

	public void SetEmitDefaultValue(bool value)
	{
		emitDefaultValue = value;
	}

	public bool GetIsRequired()
	{
		return isRequired;
	}

	public void SetIsRequired(bool value)
	{
		isRequired = value;
	}

	public string get_Name()
	{
		return name;
	}

	public void set_Name(string value)
	{
		name = value;
	}

	public int GetOrder()
	{
		return order;
	}

	public void set_Order(int value)
	{
		order = value;
	}
}
