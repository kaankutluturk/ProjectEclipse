using System;

public sealed class TypeNameInTagNodeTypeResolver : INodeTypeResolver
{
	bool INodeTypeResolver.Resolve(NodeEvent ABOEBNGCALL, ref Type PHOBEGPKAKH)
	{
		if (!string.IsNullOrEmpty(ABOEBNGCALL.GetTag()))
		{
			PHOBEGPKAKH = Type.GetType(ABOEBNGCALL.GetTag().Substring(1), true);
			return true;
		}
		return false;
	}
}
