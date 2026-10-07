using System;

public sealed class NullNodeDeserializer : INodeDeserializer
{
	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		value = null;
		NodeEvent dGMPGIHHKCN = reader.Peek<NodeEvent>();
		bool flag = dGMPGIHHKCN != null && NodeIsNull(dGMPGIHHKCN);
		if (flag)
		{
			reader.SkipThisAndNestedEvents();
		}
		return flag;
	}

	private bool NodeIsNull(NodeEvent ABOEBNGCALL)
	{
		if (ABOEBNGCALL.GetTag() == "tag:yaml.org,2002:null")
		{
			return true;
		}
		Scalar lEACOCDHICF = ABOEBNGCALL as Scalar;
		if (lEACOCDHICF == null || lEACOCDHICF.GetStyle() != ScalarStyle.Plain)
		{
			return false;
		}
		string text = lEACOCDHICF.GetValue();
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
