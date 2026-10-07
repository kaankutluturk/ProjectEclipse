using System;

public sealed class NullNodeDeserializer : INodeDeserializer
{
	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		value = null;
		NodeEvent nodeEvent = reader.Peek<NodeEvent>();
		bool flag = nodeEvent != null && NodeIsNull(nodeEvent);
		if (flag)
		{
			reader.SkipThisAndNestedEvents();
		}
		return flag;
	}

	private bool NodeIsNull(NodeEvent nodeEvent)
	{
		if (nodeEvent.GetTag() == "tag:yaml.org,2002:null")
		{
			return true;
		}
		Scalar scalar = nodeEvent as Scalar;
		if (scalar == null || scalar.GetStyle() != ScalarStyle.Plain)
		{
			return false;
		}
		string text = scalar.GetValue();
		if (text == string.Empty)
		{
			goto IL_0086;
		}
		switch (text)
		{
		case "~":
		case "null":
		case "Null":
			goto IL_0086;
		}
		int result = ((text == "NULL") ? 1 : 0);
		goto IL_0087;
		IL_0087:
		return (byte)result != 0;
		IL_0086:
		result = 1;
		goto IL_0087;
	}
}
