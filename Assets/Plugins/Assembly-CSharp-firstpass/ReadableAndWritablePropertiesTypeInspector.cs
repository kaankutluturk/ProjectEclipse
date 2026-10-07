using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ReadableAndWritablePropertiesTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeDescriptor;

	public ReadableAndWritablePropertiesTypeInspector(ITypeInspector CECGLIIIJJH)
	{
		innerTypeDescriptor = CECGLIIIJJH;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO)
	{
		return from PIIEECCHMAC in innerTypeDescriptor.GetProperties(LFLGCDNKNJI, EGJHGBCEPHO)
			where PIIEECCHMAC.GetCanWrite()
			select PIIEECCHMAC;
	}
}
