using System;

internal struct ArrayMetadata
{
	private Type element_type;

	private bool isArray;

	private bool isList;

	public Type ArrayElementType
	{
		get
		{
			return GetElementType();
		}
		set
		{
			set_ElementType(value);
		}
	}

	public bool IsArray
	{
		get
		{
			return GetIsArray();
		}
		set
		{
			SetIsArray(value);
		}
	}

	public bool IsList
	{
		get
		{
			return GetIsList();
		}
		set
		{
			SetIsList(value);
		}
	}

	public Type GetElementType()
	{
		if (element_type == null)
		{
			return typeof(JsonData);
		}
		return element_type;
	}

	public void set_ElementType(Type value)
	{
		element_type = value;
	}

	public bool GetIsArray()
	{
		return isArray;
	}

	public void SetIsArray(bool value)
	{
		isArray = value;
	}

	public bool GetIsList()
	{
		return isList;
	}

	public void SetIsList(bool value)
	{
		isList = value;
	}
}
