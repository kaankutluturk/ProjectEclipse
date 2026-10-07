using System.Collections.Generic;
using System.Xml;

public class TacticsCompiler
{
	public static void CompileTacticsSettings(XmlDocument document)
	{
		XmlNode xmlNode = document["TacticsSettings"]["Tactics"];
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (!(childNode.Name == "Tactic"))
			{
				continue;
			}
			XmlAttribute xmlAttribute = childNode.Attributes["Template"];
			if (xmlAttribute == null)
			{
				continue;
			}
			string text = xmlAttribute.GetStringOrDefault(string.Empty);
			string[] array = text.Split('|');
			List<string> list = new List<string>();
			list.AddRange(array);
			int num = array.Length;
			RemoveDuplicateTeplates(list);
			if (0 < num)
			{
				int templateIndex = 0;
				ApplyTemplates(xmlNode, childNode, list, templateIndex);
			}
			if (list.Count == 0)
			{
				continue;
			}
			string text2 = string.Empty;
			foreach (string item in list)
			{
				text2 = text2 + item + "|";
			}
			if (text2 != string.Empty)
			{
				text2 = text2.Remove(text2.Length - 1);
			}
			xmlAttribute.Value = text2;
		}
	}

	private static void ApplyTemplates(XmlNode templatesNode, XmlNode targetNode, List<string> templateNames, int index)
	{
		if (templateNames.Count <= index)
		{
			return;
		}
		string templateName = templateNames[index];
		XmlNode xmlNode = GetTemplateNode(templatesNode, templateName);
		if (xmlNode != null)
		{
			MergeAttributes(targetNode, xmlNode);
			MergeChildNodes(targetNode, xmlNode);
			XmlAttribute xmlAttribute = xmlNode.Attributes["Template"];
			if (xmlAttribute != null)
			{
				string text = xmlAttribute.GetStringOrDefault(string.Empty);
				List<string> list = new List<string>();
				string[] collection = text.Split('|');
				list.AddRange(collection);
				templateNames.AddIfNotExist(list);
			}
		}
		ApplyTemplates(templatesNode, targetNode, templateNames, index + 1);
	}

	private static XmlNode GetTemplateNode(XmlNode templatesNode, string templateName)
	{
		foreach (XmlNode childNode in templatesNode.ChildNodes)
		{
			if (childNode.Name == "Tactic")
			{
				string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				if (templateName == text)
				{
					return childNode;
				}
			}
		}
		GameLog.Error("TacticsSettings: tactics template " + templateName + " not found");
		return null;
	}

	public static void MergeAttributes(XmlNode targetNode, XmlNode sourceNode, bool overwrite = false)
	{
		foreach (XmlAttribute attribute in sourceNode.Attributes)
		{
			string name = attribute.Name;
			XmlAttribute xmlAttribute2 = targetNode.Attributes[name];
			if (xmlAttribute2 == null)
			{
				targetNode.CopyAttribute(attribute);
			}
			else if (overwrite)
			{
				xmlAttribute2.Value = attribute.Value;
			}
		}
	}

	public static void MergeChildNodes(XmlNode targetNode, XmlNode sourceNode)
	{
		foreach (XmlNode childNode in sourceNode.ChildNodes)
		{
			string name = childNode.Name;
			XmlNode xmlNode2 = targetNode[name];
			if (xmlNode2 == null)
			{
				targetNode.AppendImportedClone(childNode);
			}
		}
	}

	public static void MergeTactics(XmlNode targetSettings, XmlNode sourceSettings)
	{
		XmlNode targetTactics = targetSettings["TacticsSettings"]["Tactics"];
		XmlNode xmlNode = sourceSettings["TacticsSettings"]["Tactics"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string name = childNode.Name;
			if (name != "Tactic")
			{
				continue;
			}
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			bool flag = false;
			foreach (XmlNode childNode2 in targetSettings.ChildNodes)
			{
				if (!(name != "Tactic"))
				{
					string text2 = childNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
					if (text == text2)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				targetTactics.AppendImportedClone(childNode);
			}
		}
	}

	private static void RemoveDuplicateTeplates(List<string> templateNames)
	{
		for (int i = 0; i < templateNames.Count; i++)
		{
			for (int j = i + 1; j < templateNames.Count; j++)
			{
				if (templateNames[i] == templateNames[j])
				{
					templateNames.RemoveAt(j);
					j--;
				}
			}
		}
	}
}
