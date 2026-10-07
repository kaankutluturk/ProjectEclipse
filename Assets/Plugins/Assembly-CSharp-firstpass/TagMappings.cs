using System;
using System.Collections.Generic;

public sealed class TagMappings
{
	private readonly IDictionary<string, Type> mappings;

	public TagMappings()
	{
		mappings = new Dictionary<string, Type>();
	}

	public TagMappings(IDictionary<string, Type> sourceMappings)
	{
		this.mappings = new Dictionary<string, Type>(sourceMappings);
	}

	public void Add(string tag, Type type)
	{
		mappings.Add(tag, type);
	}

	internal Type GetMapping(string tag)
	{
		Type value;
		if (mappings.TryGetValue(tag, out value))
		{
			return value;
		}
		return null;
	}
}
