using System.Collections.Generic;
using System.Collections.ObjectModel;
using YamlDotNet.Core.Tokens;

public class TagDirectiveCollection : KeyedCollection<string, TagDirective>
{
	public TagDirectiveCollection()
	{
	}

	public TagDirectiveCollection(IEnumerable<TagDirective> tagDirectives)
	{
		foreach (TagDirective item in tagDirectives)
		{
			Add(item);
		}
	}

	protected override string GetKeyForItem(TagDirective item)
	{
		return item.Handle;
	}

	public bool Contains(TagDirective directive)
	{
		return Contains(GetKeyForItem(directive));
	}
}
