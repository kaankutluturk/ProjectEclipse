using System;
using System.Collections.Generic;
using System.Linq;

public sealed class NamingConventionTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	private readonly INamingConvention namingConvention;

	public NamingConventionTypeInspector(ITypeInspector CECGLIIIJJH, INamingConvention LELOAKPLJEH)
	{
		if (CECGLIIIJJH == null)
		{
			throw new ArgumentNullException("innerTypeDescriptor");
		}
		this.innerTypeDescriptor = CECGLIIIJJH;
		if (LELOAKPLJEH == null)
		{
			throw new ArgumentNullException("namingConvention");
		}
		this.namingConvention = LELOAKPLJEH;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO)
	{
		return innerTypeDescriptor.GetProperties(LFLGCDNKNJI, EGJHGBCEPHO).Select((Func<IPropertyDescriptor, IPropertyDescriptor>)((IPropertyDescriptor PIIEECCHMAC) =>
		{
			PropertyDescriptor fLAHDIEMBAL = new PropertyDescriptor(PIIEECCHMAC);
			fLAHDIEMBAL.set_Name(namingConvention.Apply(PIIEECCHMAC.get_Name()));
			return fLAHDIEMBAL;
		}));
	}
}
