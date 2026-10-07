using System;

public sealed class TypeNameInTagNodeTypeResolver : INodeTypeResolver
{
	bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
	{
		if (!string.IsNullOrEmpty(nodeEvent.GetTag()))
		{
			currentType = Type.GetType(nodeEvent.GetTag().Substring(1), true);
			return true;
		}
		return false;
	}
}
