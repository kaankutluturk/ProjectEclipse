using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public static class XmlUtils
{
	public enum XmlSourceMode
	{
		Normal = 0,
		ForcedResourced = 1,
		ForcedExternal = 2
	}

	public const string CommentNodeName = "#comment";

	public static bool IsCommentNode(XmlNode node)
	{
		return node.NodeType == XmlNodeType.Comment;
	}

	public static int ParseInt(this XmlAttribute attribute, int defaultValue = 0)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		int result;
		return (!int.TryParse(attribute.Value, out result)) ? defaultValue : result;
	}

	public static long ParseLong(this XmlAttribute attribute, long defaultValue = 0L)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		long result;
		return (!long.TryParse(attribute.Value, out result)) ? defaultValue : result;
	}

	public static uint ParseUint(this XmlAttribute attribute, uint defaultValue = 0u)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		uint result;
		return (!uint.TryParse(attribute.Value, out result)) ? defaultValue : result;
	}

	public static float ParseFloat(this XmlAttribute attribute, float defaultValue = 0f)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		float result;
		// Game XML always uses decimal points. The player's locale may treat a
		// point as a thousands separator, corrupting rig coordinates and weights.
		return (!float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) ? defaultValue : result;
	}

	public static bool ParseBool(this XmlAttribute attribute, bool defaultValue = false)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		int result;
		return (!int.TryParse(attribute.Value, out result)) ? defaultValue : (result > 0);
	}

	public static string ParseString(XmlAttribute attribute, string defaultValue = null)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		return attribute.Value;
	}

	public static string GetStringOrDefault(this XmlAttribute attribute, string defaultValue = null)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		return attribute.Value;
	}

	public static KeyValuePair<int, int> ParseMinMax(this XmlNode node, int defaultMin = 0, int defaultMax = 0)
	{
		if (node == null)
		{
			return new KeyValuePair<int, int>(defaultMin, defaultMax);
		}
		int key = node.Attributes["Min"].ParseInt(defaultMin);
		int value = node.Attributes["Max"].ParseInt(defaultMax);
		return new KeyValuePair<int, int>(key, value);
	}

	public static Vector2 ParseInOut(this XmlNode node, float defaultIn = 0f, float defaultOut = 0f)
	{
		Vector2 result = new Vector2(defaultIn, defaultOut);
		if (node != null)
		{
			result.x = node.Attributes["In"].ParseFloat(defaultIn);
			result.y = node.Attributes["Out"].ParseFloat(defaultOut);
		}
		return result;
	}

	public static XmlAttribute FirstAttribute(this XmlNode node)
	{
		if (node != null && node.Attributes.Count > 0)
		{
			return node.Attributes[0];
		}
		return null;
	}

	public static XmlAttribute LastAttribute(this XmlNode node)
	{
		if (node != null && node.Attributes.Count > 0)
		{
			return node.Attributes[node.Attributes.Count - 1];
		}
		return null;
	}

	public static XmlAttribute Attribute(this XmlNode node, string attributeName)
	{
		if (node != null)
		{
			return node.Attributes[attributeName];
		}
		return null;
	}

	public static T ParseEnum<T>(XmlAttribute attribute, T defaultValue)
	{
		if (attribute == null)
		{
			return defaultValue;
		}
		try
		{
			return (T)Enum.Parse(typeof(T), attribute.Value, true);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static bool Empty(this XmlAttribute attribute)
	{
		if (attribute == null)
		{
			return true;
		}
		return attribute.Value.Equals(string.Empty);
	}

	public static XmlElement AppendElement(this XmlNode node, string elementName)
	{
		XmlDocument xmlDocument = ((!(node is XmlDocument)) ? node.OwnerDocument : ((XmlDocument)node));
		XmlElement xmlElement = xmlDocument.CreateElement(elementName);
		node.AppendChild(xmlElement);
		return xmlElement;
	}

	public static XmlNode AppendImportedClone(this XmlNode node, XmlNode sourceNode)
	{
		XmlDocument xmlDocument = node.OwnerDocument;
		if (xmlDocument == null)
		{
			xmlDocument = node as XmlDocument;
		}
		XmlNode xmlNode = xmlDocument.ImportNode(sourceNode.Clone(), true);
		node.AppendChild(xmlNode);
		return xmlNode;
	}

	public static void CopyAttribute(this XmlNode node, XmlAttribute sourceAttribute)
	{
		((XmlElement)node).SetAttribute(sourceAttribute.Name, sourceAttribute.Value);
	}

	public static XmlAttribute AppendAttribute(this XmlNode node, string attributeName)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(attributeName);
		node.Attributes.Append(xmlAttribute);
		return xmlAttribute;
	}

	public static XmlAttribute PrependAttribute(this XmlNode node, string attributeName)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(attributeName);
		node.Attributes.Prepend(xmlAttribute);
		return xmlAttribute;
	}

	public static XmlNode AppendNewNode(this XmlDocument document, string nodeName)
	{
		XmlNode xmlNode = document.CreateNode(XmlNodeType.Element, nodeName, null);
		document.AppendChild(xmlNode);
		return xmlNode;
	}

	public static XmlNode AppendNewNode(this XmlNode node, string nodeName)
	{
		XmlNode xmlNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, null);
		node.AppendChild(xmlNode);
		return xmlNode;
	}

	public static XmlNode FindChildWithAttribute(this XmlNode node, string name, string attributeName)
	{
		if (node != null)
		{
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name.Equals(name))
				{
					XmlAttribute xmlAttribute = childNode.Attributes[attributeName];
					if (xmlAttribute != null)
					{
						return childNode;
					}
				}
			}
		}
		return null;
	}

	public static XmlNode FindChildWithAttribute(this XmlNode node, string name, string attributeName, string attributeValue)
	{
		if (node != null)
		{
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name.Equals(name))
				{
					XmlAttribute xmlAttribute = childNode.FindAttributeWithValue(attributeName, attributeValue);
					if (xmlAttribute != null)
					{
						return childNode;
					}
				}
			}
		}
		return null;
	}

	public static XmlAttribute FindAttributeWithValue(this XmlNode node, string attributeName, string attributeValue)
	{
		if (node != null)
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				if (attribute.Name.Equals(attributeName) && attribute.Value.Equals(attributeValue))
				{
					return attribute;
				}
			}
		}
		return null;
	}

	public static XmlDocument LoadFromBytes(byte[] bytes, bool ignoreComments = true)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = ignoreComments;
		try
		{
			MemoryStream stream = new MemoryStream(bytes);
			using (XmlReader reader = XmlReader.Create(stream, xmlReaderSettings))
			{
				return OpenXMLDocument(reader);
			}
		}
		catch
		{
			return null;
		}
	}

	public static XmlDocument LoadFromString(string xml, bool ignoreComments = true, bool decryptIfNeeded = false)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = ignoreComments;
		try
		{
			XmlDocument xmlDocument = ParseXmlText(xml, xmlReaderSettings, decryptIfNeeded);
			if (xmlDocument == null)
			{
				Debug.LogError("Error open xml from string: " + xml);
			}
			return xmlDocument;
		}
		catch
		{
			return null;
		}
	}

	public static XmlDocument OpenXMLDocument(string basePath, string fileName = "", XmlSourceMode sourceMode = XmlSourceMode.Normal, bool ignoreComments = true, bool decryptIfNeeded = false)
	{
		try
		{
			string text = basePath + ((!(fileName != string.Empty)) ? string.Empty : ("/" + fileName));
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = ignoreComments;
			// TexturePacker emits Apple plist files with a DOCTYPE declaration.
			// These files never need external entity resolution, but rejecting the
			// declaration makes every recovered location atlas look empty.
			xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
			xmlReaderSettings.XmlResolver = null;
			XmlDocument xmlDocument = null;
			switch (sourceMode)
			{
			case XmlSourceMode.Normal:
				xmlDocument = ((!basePath.StartsWith(SF2Paths.UserDataRoot)) ? ParseXmlText(ResourceManager.GetText(text), xmlReaderSettings, decryptIfNeeded) : ParseXmlText(ResourceManager.GetFileOrDevText(text), xmlReaderSettings, decryptIfNeeded));
				break;
			case XmlSourceMode.ForcedExternal:
				xmlDocument = ParseXmlText(ResourceManager.GetFileOrDevText(text), xmlReaderSettings, decryptIfNeeded);
				break;
			case XmlSourceMode.ForcedResourced:
				xmlDocument = ParseXmlText(ResourceManager.GetBundledOrModText(text), xmlReaderSettings, decryptIfNeeded);
				break;
			}
			if (xmlDocument == null)
			{
				Debug.LogError("Error open xml " + text);
			}
			return xmlDocument;
		}
		catch
		{
			return null;
		}
	}

	private static XmlDocument ParseXmlText(string xmlText, XmlReaderSettings readerSettings, bool decryptIfNeeded)
	{
		if (decryptIfNeeded && (string.IsNullOrEmpty(xmlText) || !xmlText.TrimStart().StartsWith("<")))
		{
			string text = XmlCryptoUtils.DecryptStringOrPassThrough(xmlText);
			if (!string.IsNullOrEmpty(text))
			{
				xmlText = text;
			}
		}
		using (XmlReader reader = XmlReader.Create(new StringReader(xmlText), readerSettings))
		{
			return OpenXMLDocument(reader);
		}
	}

	private static XmlDocument OpenXMLDocument(XmlReader reader)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(reader);
			return xmlDocument;
		}
		catch (Exception ex)
		{
			GameLog.Write(ex.ToString());
			return null;
		}
	}

	public static void TrimWhitespaceToFile(string sourcePath, string destinationPath)
	{
		XmlDocument xmlDocument = OpenXMLDocument(sourcePath, string.Empty, XmlSourceMode.ForcedExternal);
		if (xmlDocument == null)
		{
			Debug.LogError("[XmlUtils]: try to trim spaces from incorrect xml - " + sourcePath);
			return;
		}
		try
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = false;
			xmlWriterSettings.NewLineChars = string.Empty;
			using (XmlWriter xmlWriter = XmlWriter.Create(destinationPath, xmlWriterSettings))
			{
				xmlDocument.Save(xmlWriter);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void TrimWhitespaceInPlace(string filePath)
	{
		TrimWhitespaceToFile(filePath, filePath);
	}

	public static string ToIndentedString(this XmlDocument document)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
		{
			document.Save(xmlWriter);
		}
		return stringBuilder.ToString();
	}

	public static XmlDocument LoadDocumentWithHashCheck(string basePath, string fileName = "", XmlSourceMode sourceMode = XmlSourceMode.Normal, bool ignoreComments = true)
	{
		if (sourceMode == XmlSourceMode.Normal || sourceMode == XmlSourceMode.ForcedExternal)
			Eclipse.Modding.ModRuntime.RecoverProfileSnapshot(string.IsNullOrEmpty(fileName) ? basePath : Path.Combine(basePath, fileName));
		XmlDocument xmlDocument = OpenXMLDocument(basePath, fileName, sourceMode, ignoreComments);
		if (xmlDocument != null && (sourceMode == XmlSourceMode.Normal || sourceMode == XmlSourceMode.ForcedExternal))
		{
			string fullPath = basePath + ((!(fileName != string.Empty)) ? string.Empty : ("/" + fileName));
			UserDataValidator.CheckFileHash(xmlDocument, fullPath);
		}
		return xmlDocument;
	}

	public static void SaveDocumentWithHash(XmlDocument document, string filePath)
	{
		if (Eclipse.Modding.ModRuntime.TryWriteProfileSnapshot(document, filePath)) return;
		document.Save(filePath);
		UserDataValidator.UpdateFileHash(document, filePath);
	}

	public static void CopyDocumentWithHash(string sourcePath, string destinationPath)
	{
		XmlDocument xmlDocument = OpenXMLDocument(sourcePath, string.Empty);
		xmlDocument.Save(destinationPath);
		UserDataValidator.UpdateFileHash(xmlDocument, destinationPath);
	}

	public static void UpdateHashForFile(string filePath)
	{
		try
		{
			if (Path.GetExtension(filePath).ToLower().Equals(".xml"))
			{
				XmlDocument document = OpenXMLDocument(filePath, string.Empty);
				UserDataValidator.UpdateFileHash(document, filePath);
			}
			else
			{
				UserDataValidator.UpdateFileHash(filePath);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Exception: " + ex.Message);
		}
	}
}
