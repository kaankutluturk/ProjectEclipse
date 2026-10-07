using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Core;

[DefaultMember("Item")]
internal sealed class ObjectAnchorCollection
{
	private readonly IDictionary<string, object> objectsByAnchor = new Dictionary<string, object>();

	private readonly IDictionary<object, string> anchorsByObject = new Dictionary<object, string>();

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public object get_DLKPBAJDHBO(string anchor)
	{
		return get_Item(anchor);
	}

	public void Add(string anchor, object obj)
	{
		objectsByAnchor.Add(anchor, obj);
		if (obj != null)
		{
			anchorsByObject.Add(obj, anchor);
		}
	}

	public bool TryGetAnchor(object obj, out string anchor)
	{
		return anchorsByObject.TryGetValue(obj, out anchor);
	}

	public object get_Item(string anchor)
	{
		object value;
		if (objectsByAnchor.TryGetValue(anchor, out value))
		{
			return value;
		}
		throw new AnchorNotFoundException(string.Format(CultureInfo.InvariantCulture, "The anchor '{0}' does not exists", anchor));
	}
}
