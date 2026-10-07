using System;
using System.Globalization;
using System.Linq;

public class RoundtripObjectGraphTraversalStrategy : FullObjectGraphTraversalStrategy
{
	public RoundtripObjectGraphTraversalStrategy(Serializer serializer, ITypeInspector typeInspector, ITypeResolver typeResolver, int maxRecursion)
		: base(serializer, typeInspector, typeResolver, maxRecursion)
	{
	}

	protected override void TraverseProperties(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
	{
		if (!value.get_Type().HasDefaultConstructor() && !serializer.GetConverters().Any((IYamlTypeConverter converter) => converter.Accepts(value.get_Type())))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor or a type converter.", value.get_Type()));
		}
		base.TraverseProperties(value, visitor, currentDepth);
	}
}
