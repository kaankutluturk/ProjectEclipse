using System;
using System.Collections.Generic;

internal struct ObjectMetadata
{
	private Type element_type;

	private bool is_dictionary;

	private IDictionary<string, PropertyMetadata> properties;

	public Type ItemType
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

	public bool IsDictionaryType
	{
		get
		{
			return GetIsDictionary();
		}
		set
		{
			set_IsDictionary(value);
		}
	}

	public IDictionary<string, PropertyMetadata> Properties
	{
		get
		{
			return GetProperties();
		}
		set
		{
			SetProperties(value);
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

	public bool GetIsDictionary()
	{
		return is_dictionary;
	}

	public void set_IsDictionary(bool value)
	{
		is_dictionary = value;
	}

	public IDictionary<string, PropertyMetadata> GetProperties()
	{
		return properties;
	}

	public void SetProperties(IDictionary<string, PropertyMetadata> value)
	{
		properties = value;
	}
}
