using System;
using System.Collections.Generic;
using System.ComponentModel;

public sealed class DefaultExclusiveObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private static readonly IEqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

	public DefaultExclusiveObjectGraphVisitor(IObjectGraphVisitor GDMFLLGPLNO)
		: base(GDMFLLGPLNO)
	{
	}

	private static object GetDefault(Type LFLGCDNKNJI)
	{
		return (!LFLGCDNKNJI.IsValueTypeCheck()) ? null : Activator.CreateInstance(LFLGCDNKNJI);
	}

	public override bool EnterMapping(IObjectDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return !_objectComparer.Equals(value, GetDefault(value.get_Type())) && base.EnterMapping(KGBGENDIMBC, value);
	}

	public override bool EnterMapping(IPropertyDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		DefaultValueAttribute defaultValueAttribute = KGBGENDIMBC.GetCustomAttribute<DefaultValueAttribute>();
		object y = ((defaultValueAttribute == null) ? GetDefault(KGBGENDIMBC.get_Type()) : defaultValueAttribute.Value);
		return !_objectComparer.Equals(value.GetValue(), y) && base.EnterMapping(KGBGENDIMBC, value);
	}
}
