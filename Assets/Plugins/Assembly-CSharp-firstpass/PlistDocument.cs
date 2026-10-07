using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

public class PlistDocument
{
	public PlistElementDict root;

	public string version;

	public PlistDocument()
	{
		root = new PlistElementDict();
		version = "1.0";
	}

	internal static XDocument ParseXmlNoDtd(string xml)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.ProhibitDtd = false;
		xmlReaderSettings.XmlResolver = null;
		XmlReader reader = XmlReader.Create(new StringReader(xml), xmlReaderSettings);
		return XDocument.Load(reader);
	}

	internal static string CleanDtdToString(XDocument document)
	{
		if (document.DocumentType != null)
		{
			XDocument xDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), new XDocumentType(document.DocumentType.Name, document.DocumentType.PublicId, document.DocumentType.SystemId, null), new XElement(document.Root.Name));
			return string.Concat(string.Empty, xDocument.Declaration, "\n", xDocument.DocumentType, "\n", document.Root);
		}
		XDocument xDocument2 = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(document.Root.Name));
		return string.Concat(string.Empty, xDocument2.Declaration, Environment.NewLine, document.Root);
	}

	private static string GetText(XElement element)
	{
		return string.Join(string.Empty, (from textNode in element.Nodes().OfType<XText>()
			select textNode.Value).ToArray());
	}

	private static PlistElement ParseValue(XElement element)
	{
		switch (element.Name.LocalName)
		{
		case "dict":
		{
			List<XElement> list2 = element.Elements().ToList();
			PlistElementDict dict = new PlistElementDict();
			if (list2.Count % 2 == 1)
			{
				throw new Exception("Malformed plist file");
			}
			for (int i = 0; i < list2.Count - 1; i++)
			{
				if (list2[i].Name != "key")
				{
					throw new Exception("Malformed plist file");
				}
				string key = GetText(list2[i]).Trim();
				PlistElement parsedElement = ParseValue(list2[i + 1]);
				if (parsedElement != null)
				{
					i++;
					dict.SetItem(key, parsedElement);
				}
			}
			return dict;
		}
		case "array":
		{
			List<XElement> list = element.Elements().ToList();
			PlistElementArray array = new PlistElementArray();
			{
				foreach (XElement item in list)
				{
					PlistElement childValue = ParseValue(item);
					if (childValue != null)
					{
						array.values.Add(childValue);
					}
				}
				return array;
			}
		}
		case "string":
			return new PlistElementString(GetText(element));
		case "integer":
		{
			int result;
			if (int.TryParse(GetText(element), out result))
			{
				return new PlistElementInteger(result);
			}
			return null;
		}
		case "true":
			return new PlistElementBoolean(true);
		case "false":
			return new PlistElementBoolean(false);
		default:
			return null;
		}
	}

	public void ReadFromFile(string path)
	{
		ReadFromString(File.ReadAllText(path));
	}

	public void ReadFromStream(TextReader reader)
	{
		ReadFromString(reader.ReadToEnd());
	}

	public void ReadFromString(string xml)
	{
		XDocument xDocument = ParseXmlNoDtd(xml);
		version = (string)xDocument.Root.Attribute("version");
		XElement rootElement = xDocument.XPathSelectElement("plist/dict");
		PlistElement rootValue = ParseValue(rootElement);
		if (rootValue == null)
		{
			throw new Exception("Error parsing plist file");
		}
		root = rootValue as PlistElementDict;
		if (root == null)
		{
			throw new Exception("Malformed plist file");
		}
	}

	private static XElement WriteElement(PlistElement element)
	{
		if (element is PlistElementBoolean)
		{
			PlistElementBoolean boolElement = element as PlistElementBoolean;
			return new XElement((!boolElement.value) ? "false" : "true");
		}
		if (element is PlistElementInteger)
		{
			PlistElementInteger integerElement = element as PlistElementInteger;
			return new XElement("integer", integerElement.value.ToString());
		}
		if (element is PlistElementString)
		{
			PlistElementString stringElement = element as PlistElementString;
			return new XElement("string", stringElement.value);
		}
		if (element is PlistElementDict)
		{
			PlistElementDict dict = element as PlistElementDict;
			XElement xElement = new XElement("dict");
			{
				foreach (KeyValuePair<string, PlistElement> item in dict.GetValues())
				{
					XElement content = new XElement("key", item.Key);
					XElement xElement2 = WriteElement(item.Value);
					if (xElement2 != null)
					{
						xElement.Add(content);
						xElement.Add(xElement2);
					}
				}
				return xElement;
			}
		}
		if (element is PlistElementArray)
		{
			PlistElementArray array = element as PlistElementArray;
			XElement xElement3 = new XElement("array");
			{
				foreach (PlistElement item2 in array.values)
				{
					XElement xElement4 = WriteElement(item2);
					if (xElement4 != null)
					{
						xElement3.Add(xElement4);
					}
				}
				return xElement3;
			}
		}
		return null;
	}

	public void WriteToFile(string path)
	{
		File.WriteAllText(path, WriteToString());
	}

	public void WriteToStream(TextWriter writer)
	{
		writer.Write(WriteToString());
	}

	public string WriteToString()
	{
		XElement content = WriteElement(root);
		XElement xElement = new XElement("plist");
		xElement.Add(new XAttribute("version", version));
		xElement.Add(content);
		XDocument xDocument = new XDocument();
		xDocument.Add(xElement);
		return CleanDtdToString(xDocument);
	}
}
