using System;
using System.Collections.Generic;
using System.Linq;

public sealed class YamlAttributesTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	public YamlAttributesTypeInspector(ITypeInspector CECGLIIIJJH)
	{
		this.innerTypeDescriptor = CECGLIIIJJH;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO)
	{
		return from PIIEECCHMAC in (from PIIEECCHMAC in innerTypeDescriptor.GetProperties(LFLGCDNKNJI, EGJHGBCEPHO)
				where PIIEECCHMAC.GetCustomAttribute<YamlIgnoreAttribute>() == null
				select PIIEECCHMAC).Select((Func<IPropertyDescriptor, IPropertyDescriptor>)((IPropertyDescriptor PIIEECCHMAC) =>
			{
				PropertyDescriptor fLAHDIEMBAL = new PropertyDescriptor(PIIEECCHMAC);
				YamlAliasAttribute gAANEGEKJGH = PIIEECCHMAC.GetCustomAttribute<YamlAliasAttribute>();
				if (gAANEGEKJGH != null)
				{
					fLAHDIEMBAL.set_Name(gAANEGEKJGH.GetAlias());
				}
				YamlMemberAttribute kGBEBCLPIIO = PIIEECCHMAC.GetCustomAttribute<YamlMemberAttribute>();
				if (kGBEBCLPIIO != null)
				{
					if (kGBEBCLPIIO.GetSerializeAs() != null)
					{
						fLAHDIEMBAL.set_TypeOverride(kGBEBCLPIIO.GetSerializeAs());
					}
					fLAHDIEMBAL.set_Order(kGBEBCLPIIO.GetOrder());
					if (kGBEBCLPIIO.GetName() != null)
					{
						if (gAANEGEKJGH != null)
						{
							throw new InvalidOperationException("Mixing YamlAlias(...) with YamlMember(Alias = ...) is an error. The YamlAlias attribute is obsolete and should be removed.");
						}
						fLAHDIEMBAL.set_Name(kGBEBCLPIIO.GetName());
					}
				}
				return fLAHDIEMBAL;
			}))
			orderby PIIEECCHMAC.GetOrder()
			select PIIEECCHMAC;
	}
}
