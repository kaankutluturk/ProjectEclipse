using System;
using System.Collections.Generic;
using System.ComponentModel;

public sealed class DefaultExclusiveObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private static readonly IEqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

	public DefaultExclusiveObjectGraphVisitor(IObjectGraphVisitor visitor)
		: base(visitor)
	{
	}

	private static object GetDefault(Type type)
	{
		return (!type.IsValueTypeCheck()) ? null : Activator.CreateInstance(type);
	}

	public override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value)
	{
		return !_objectComparer.Equals(value, GetDefault(value.get_Type())) && base.EnterMapping(key, value);
	}

	public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value)
	{
		DefaultValueAttribute defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
		object y = ((defaultValueAttribute == null) ? GetDefault(key.get_Type()) : defaultValueAttribute.Value);
		return !_objectComparer.Equals(value.GetValue(), y) && base.EnterMapping(key, value);
	}
}
