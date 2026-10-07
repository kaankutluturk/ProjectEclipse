using System.Collections.Generic;
using System.Xml;

public static class PerksCompiler
{
	public static void CompilePerks(ref XmlDocument document, string resourceName)
	{
		document = XmlUtils.OpenXMLDocument(resourceName, string.Empty, XmlUtils.XmlSourceMode.ForcedResourced);
		if (document != null)
		{
			XmlNode xmlNode = document["Perks"];
			if (xmlNode != null)
			{
				foreach (XmlNode childNode in xmlNode.ChildNodes)
				{
					if (!childNode.Name.Equals("Perk"))
					{
						continue;
					}
					List<string> list = new List<string>();
					ResolveTemplates(childNode, xmlNode, list);
					XmlAttribute xmlAttribute = childNode.Attributes["Template"];
					if (xmlAttribute != null && list.Count > 0)
					{
						string text = string.Empty;
						foreach (string item in list)
						{
							text = string.Format("{0}{1}|", text, item);
						}
						if (!string.IsNullOrEmpty(text))
						{
							text = text.Remove(text.Length - 1);
						}
						xmlAttribute.Value = text;
					}
					list.Clear();
				}
			}
		}
		AddIDs(document);
		if (SystemProperties.IsDebug())
		{
			document.Save(string.Format("{0}/{1}", SF2Paths.GetWritableGameDataPath(), "perks_result.xml"));
		}
	}

	private static XmlNode GetTemplateNode(XmlNode templatesNode, string templateName)
	{
		foreach (XmlNode childNode in templatesNode.ChildNodes)
		{
			if (childNode.Name.Equals("Perk"))
			{
				string value = childNode.Attributes["Name"].GetStringOrDefault();
				if (templateName.Equals(value))
				{
					return childNode;
				}
			}
		}
		GameLog.Error("Perks: tactics template '{0}' not found", templateName);
		return null;
	}

	private static void CopyMissingAttributes(XmlNode targetNode, XmlNode sourceNode)
	{
		foreach (XmlAttribute attribute in sourceNode.Attributes)
		{
			string name = attribute.Name;
			XmlAttribute xmlAttribute2 = targetNode.Attributes[name];
			if (xmlAttribute2 == null)
			{
				targetNode.CopyAttribute(attribute);
			}
		}
	}

	private static void MergeTemplateChildren(XmlNode targetNode, XmlNode sourceNode)
	{
		foreach (XmlNode childNode in sourceNode.ChildNodes)
		{
			string name = childNode.Name;
			XmlNode xmlNode2 = targetNode["Set"];
			if (name == "Trigger" || (name == "Set" && xmlNode2 == null))
			{
				targetNode.AppendImportedClone(childNode);
			}
			else if (name == "Set" && xmlNode2 != null)
			{
				CopyMissingAttributes(xmlNode2, childNode);
			}
		}
	}

	private static void ResolveTemplates(XmlNode node, XmlNode templatesNode, List<string> visitedTemplates)
	{
		List<string> list = new List<string>();
		XmlAttribute xmlAttribute = node.Attributes["Template"];
		if (xmlAttribute == null)
		{
			return;
		}
		string text = xmlAttribute.GetStringOrDefault();
		list.AddRange(text.Split('|'));
		list = list.GetDistinct();
		foreach (string item in list)
		{
			XmlNode xmlNode = GetTemplateNode(templatesNode, item);
			if (xmlNode != null)
			{
				XmlDocument document = new XmlDocument();
				XmlNode xmlNode2 = document.AppendImportedClone(xmlNode);
				ResolveTemplates(xmlNode2, templatesNode, visitedTemplates);
				CopyMissingAttributes(node, xmlNode2);
				MergeTemplateChildren(node, xmlNode2);
			}
			visitedTemplates.Add(item);
		}
	}

	private static void AddIDs(XmlDocument document)
	{
		XmlNode xmlNode = document["Perks"];
		int num = 0;
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name.Equals("Perk"))
			{
				childNode.PrependAttribute("ID").Value = num.ToString();
				num++;
			}
		}
	}
}
