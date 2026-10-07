using System.IO;
using System.Xml;

public class DeflatedString
{
	private XmlNode _Node;

	public void Set(XmlNode node)
	{
		_Node = node;
	}

	public XmlNode GetNode()
	{
		return _Node;
	}

	public static string NodeToString(XmlNode node, int indentation = 0)
	{
		using (StringWriter stringWriter = new StringWriter())
		{
			using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
			{
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.Indentation = indentation;
				node.WriteTo(xmlTextWriter);
			}
			return stringWriter.ToString();
		}
	}
}
