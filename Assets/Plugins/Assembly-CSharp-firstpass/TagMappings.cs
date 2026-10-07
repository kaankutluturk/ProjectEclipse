using System;
using System.Collections.Generic;

public sealed class TagMappings
{
	private readonly IDictionary<string, Type> mappings;

	public TagMappings()
	{
		mappings = new Dictionary<string, Type>();
	}

	public TagMappings(IDictionary<string, Type> INONLMCLKPG)
	{
		this.mappings = new Dictionary<string, Type>(INONLMCLKPG);
	}

	public void Add(string EDLADAAKMDF, Type JPEFEBICPFI)
	{
		mappings.Add(EDLADAAKMDF, JPEFEBICPFI);
	}

	internal Type GetMapping(string EDLADAAKMDF)
	{
		Type value;
		if (mappings.TryGetValue(EDLADAAKMDF, out value))
		{
			return value;
		}
		return null;
	}
}
