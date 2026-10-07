using System;
using System.Collections.Generic;

public sealed class TagNodeTypeResolver : INodeTypeResolver
{
	private readonly IDictionary<string, Type> tagMappings;

	public TagNodeTypeResolver(IDictionary<string, Type> NKEHCGOLJDA)
	{
		if (NKEHCGOLJDA == null)
		{
			throw new ArgumentNullException("tagMappings");
		}
		this.tagMappings = NKEHCGOLJDA;
	}

	bool INodeTypeResolver.Resolve(NodeEvent ABOEBNGCALL, ref Type PHOBEGPKAKH)
	{
		Type value;
		if (!string.IsNullOrEmpty(ABOEBNGCALL.GetTag()) && tagMappings.TryGetValue(ABOEBNGCALL.GetTag(), out value))
		{
			PHOBEGPKAKH = value;
			return true;
		}
		return false;
	}
}
