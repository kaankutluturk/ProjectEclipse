using System;
using System.Collections.Generic;

public sealed class DefaultContainersNodeTypeResolver : INodeTypeResolver
{
	bool INodeTypeResolver.Resolve(NodeEvent ABOEBNGCALL, ref Type PHOBEGPKAKH)
	{
		if (PHOBEGPKAKH == typeof(object))
		{
			if (ABOEBNGCALL is SequenceStart)
			{
				PHOBEGPKAKH = typeof(List<object>);
				return true;
			}
			if (ABOEBNGCALL is MappingStart)
			{
				PHOBEGPKAKH = typeof(Dictionary<object, object>);
				return true;
			}
		}
		return false;
	}
}
