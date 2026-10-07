using System;
using System.Collections.Generic;

public sealed class TagNodeTypeResolver : INodeTypeResolver
{
	private readonly IDictionary<string, Type> tagMappings;

	public TagNodeTypeResolver(IDictionary<string, Type> mappings)
	{
		if (mappings == null)
		{
			throw new ArgumentNullException("tagMappings");
		}
		this.tagMappings = mappings;
	}

	bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
	{
		Type value;
		if (!string.IsNullOrEmpty(nodeEvent.GetTag()) && tagMappings.TryGetValue(nodeEvent.GetTag(), out value))
		{
			currentType = value;
			return true;
		}
		return false;
	}
}
