using System;
using System.Collections.Generic;
using System.Linq;

public sealed class YamlAttributesTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	public YamlAttributesTypeInspector(ITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
	{
		return from property in (from property in innerTypeDescriptor.GetProperties(type, container)
				where property.GetCustomAttribute<YamlIgnoreAttribute>() == null
				select property).Select((Func<IPropertyDescriptor, IPropertyDescriptor>)((IPropertyDescriptor property) =>
			{
				PropertyDescriptor wrappedProperty = new PropertyDescriptor(property);
				YamlAliasAttribute aliasAttribute = property.GetCustomAttribute<YamlAliasAttribute>();
				if (aliasAttribute != null)
				{
					wrappedProperty.set_Name(aliasAttribute.GetAlias());
				}
				YamlMemberAttribute memberAttribute = property.GetCustomAttribute<YamlMemberAttribute>();
				if (memberAttribute != null)
				{
					if (memberAttribute.GetSerializeAs() != null)
					{
						wrappedProperty.set_TypeOverride(memberAttribute.GetSerializeAs());
					}
					wrappedProperty.set_Order(memberAttribute.GetOrder());
					if (memberAttribute.GetName() != null)
					{
						if (aliasAttribute != null)
						{
							throw new InvalidOperationException("Mixing YamlAlias(...) with YamlMember(Alias = ...) is an error. The YamlAlias attribute is obsolete and should be removed.");
						}
						wrappedProperty.set_Name(memberAttribute.GetName());
					}
				}
				return wrappedProperty;
			}))
			orderby property.GetOrder()
			select property;
	}
}
