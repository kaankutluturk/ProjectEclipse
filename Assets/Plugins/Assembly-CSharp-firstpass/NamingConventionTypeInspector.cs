using System;
using System.Collections.Generic;
using System.Linq;

public sealed class NamingConventionTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	private readonly INamingConvention namingConvention;

	public NamingConventionTypeInspector(ITypeInspector innerTypeDescriptor, INamingConvention namingConvention)
	{
		if (innerTypeDescriptor == null)
		{
			throw new ArgumentNullException("innerTypeDescriptor");
		}
		this.innerTypeDescriptor = innerTypeDescriptor;
		if (namingConvention == null)
		{
			throw new ArgumentNullException("namingConvention");
		}
		this.namingConvention = namingConvention;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
	{
		return innerTypeDescriptor.GetProperties(type, container).Select((Func<IPropertyDescriptor, IPropertyDescriptor>)((IPropertyDescriptor innerDescriptor) =>
		{
			PropertyDescriptor descriptor = new PropertyDescriptor(innerDescriptor);
			descriptor.set_Name(namingConvention.Apply(innerDescriptor.get_Name()));
			return descriptor;
		}));
	}
}
