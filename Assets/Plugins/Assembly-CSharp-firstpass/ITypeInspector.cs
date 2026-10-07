using System;
using System.Collections.Generic;

public interface ITypeInspector
{
	IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO);

	IPropertyDescriptor GetProperty(Type LFLGCDNKNJI, object EGJHGBCEPHO, string name, bool GNFDAJLHBCN);
}
