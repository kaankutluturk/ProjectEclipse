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

	public static bool IsCommentNode(XmlNode MEEAKLDGLDF)
	{
		return MEEAKLDGLDF.NodeType == XmlNodeType.Comment;
	}

	public static int ParseInt(this XmlAttribute CJBEMNNNHDM, int KDLNPAGLMHF = 0)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		int result;
		return (!int.TryParse(CJBEMNNNHDM.Value, out result)) ? KDLNPAGLMHF : result;
	}

	public static long ParseLong(this XmlAttribute CJBEMNNNHDM, long KDLNPAGLMHF = 0L)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		long result;
		return (!long.TryParse(CJBEMNNNHDM.Value, out result)) ? KDLNPAGLMHF : result;
	}

	public static uint ParseUint(this XmlAttribute CJBEMNNNHDM, uint KDLNPAGLMHF = 0u)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		uint result;
		return (!uint.TryParse(CJBEMNNNHDM.Value, out result)) ? KDLNPAGLMHF : result;
	}

	public static float ParseFloat(this XmlAttribute CJBEMNNNHDM, float KDLNPAGLMHF = 0f)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		float result;
		// Game XML always uses decimal points. The player's locale may treat a
		// point as a thousands separator, corrupting rig coordinates and weights.
		return (!float.TryParse(CJBEMNNNHDM.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) ? KDLNPAGLMHF : result;
	}

	public static bool ParseBool(this XmlAttribute CJBEMNNNHDM, bool KDLNPAGLMHF = false)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		int result;
		return (!int.TryParse(CJBEMNNNHDM.Value, out result)) ? KDLNPAGLMHF : (result > 0);
	}

	public static string ParseString(XmlAttribute CJBEMNNNHDM, string KDLNPAGLMHF = null)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		return CJBEMNNNHDM.Value;
	}

	public static string GetStringOrDefault(this XmlAttribute CJBEMNNNHDM, string KDLNPAGLMHF = null)
	{
		if (CJBEMNNNHDM == null)
		{
			return KDLNPAGLMHF;
		}
		return CJBEMNNNHDM.Value;
	}

	public static KeyValuePair<int, int> ParseMinMax(this XmlNode MEEAKLDGLDF, int JLJBICAKJJH = 0, int PPHPIDGJOCJ = 0)
	{
		if (MEEAKLDGLDF == null)
		{
			return new KeyValuePair<int, int>(JLJBICAKJJH, PPHPIDGJOCJ);
		}
		int key = MEEAKLDGLDF.Attributes["Min"].ParseInt(JLJBICAKJJH);
		int value = MEEAKLDGLDF.Attributes["Max"].ParseInt(PPHPIDGJOCJ);
		return new KeyValuePair<int, int>(key, value);
	}

	public static Vector2 ParseInOut(this XmlNode MEEAKLDGLDF, float COPBJEEJIBB = 0f, float FLOKDJLEJCK = 0f)
	{
		Vector2 result = new Vector2(COPBJEEJIBB, FLOKDJLEJCK);
		if (MEEAKLDGLDF != null)
		{
			result.x = MEEAKLDGLDF.Attributes["In"].ParseFloat(COPBJEEJIBB);
			result.y = MEEAKLDGLDF.Attributes["Out"].ParseFloat(FLOKDJLEJCK);
		}
		return result;
	}

	public static XmlAttribute FirstAttribute(this XmlNode MEEAKLDGLDF)
	{
		if (MEEAKLDGLDF != null && MEEAKLDGLDF.Attributes.Count > 0)
		{
			return MEEAKLDGLDF.Attributes[0];
		}
		return null;
	}

	public static XmlAttribute LastAttribute(this XmlNode MEEAKLDGLDF)
	{
		if (MEEAKLDGLDF != null && MEEAKLDGLDF.Attributes.Count > 0)
		{
			return MEEAKLDGLDF.Attributes[MEEAKLDGLDF.Attributes.Count - 1];
		}
		return null;
	}

	public static XmlAttribute Attribute(this XmlNode MEEAKLDGLDF, string PNPLADGGOJN)
	{
		if (MEEAKLDGLDF != null)
		{
			return MEEAKLDGLDF.Attributes[PNPLADGGOJN];
		}
		return null;
	}

	public static T ParseEnum<T>(XmlAttribute CJBEMNNNHDM, T JEALBOJLKFM)
	{
		if (CJBEMNNNHDM == null)
		{
			return JEALBOJLKFM;
		}
		try
		{
			return (T)Enum.Parse(typeof(T), CJBEMNNNHDM.Value, true);
		}
		catch
		{
			return JEALBOJLKFM;
		}
	}

	public static bool Empty(this XmlAttribute CJBEMNNNHDM)
	{
		if (CJBEMNNNHDM == null)
		{
			return true;
		}
		return CJBEMNNNHDM.Value.Equals(string.Empty);
	}

	public static XmlElement AppendElement(this XmlNode MEEAKLDGLDF, string JLEKBBJBLOE)
	{
		XmlDocument xmlDocument = ((!(MEEAKLDGLDF is XmlDocument)) ? MEEAKLDGLDF.OwnerDocument : ((XmlDocument)MEEAKLDGLDF));
		XmlElement xmlElement = xmlDocument.CreateElement(JLEKBBJBLOE);
		MEEAKLDGLDF.AppendChild(xmlElement);
		return xmlElement;
	}

	public static XmlNode AppendImportedClone(this XmlNode MEEAKLDGLDF, XmlNode NBMGOEMJJAF)
	{
		XmlDocument xmlDocument = MEEAKLDGLDF.OwnerDocument;
		if (xmlDocument == null)
		{
			xmlDocument = MEEAKLDGLDF as XmlDocument;
		}
		XmlNode xmlNode = xmlDocument.ImportNode(NBMGOEMJJAF.Clone(), true);
		MEEAKLDGLDF.AppendChild(xmlNode);
		return xmlNode;
	}

	public static void CopyAttribute(this XmlNode MEEAKLDGLDF, XmlAttribute NBMGOEMJJAF)
	{
		((XmlElement)MEEAKLDGLDF).SetAttribute(NBMGOEMJJAF.Name, NBMGOEMJJAF.Value);
	}

	public static XmlAttribute AppendAttribute(this XmlNode MEEAKLDGLDF, string MJMEBBCLHII)
	{
		XmlAttribute xmlAttribute = MEEAKLDGLDF.OwnerDocument.CreateAttribute(MJMEBBCLHII);
		MEEAKLDGLDF.Attributes.Append(xmlAttribute);
		return xmlAttribute;
	}

	public static XmlAttribute PrependAttribute(this XmlNode MEEAKLDGLDF, string MJMEBBCLHII)
	{
		XmlAttribute xmlAttribute = MEEAKLDGLDF.OwnerDocument.CreateAttribute(MJMEBBCLHII);
		MEEAKLDGLDF.Attributes.Prepend(xmlAttribute);
		return xmlAttribute;
	}

	public static XmlNode AppendNewNode(this XmlDocument JMCOLDENNDH, string IMGCANJHPND)
	{
		XmlNode xmlNode = JMCOLDENNDH.CreateNode(XmlNodeType.Element, IMGCANJHPND, null);
		JMCOLDENNDH.AppendChild(xmlNode);
		return xmlNode;
	}

	public static XmlNode AppendNewNode(this XmlNode MEEAKLDGLDF, string IMGCANJHPND)
	{
		XmlNode xmlNode = MEEAKLDGLDF.OwnerDocument.CreateNode(XmlNodeType.Element, IMGCANJHPND, null);
		MEEAKLDGLDF.AppendChild(xmlNode);
		return xmlNode;
	}

	public static XmlNode FindChildWithAttribute(this XmlNode MEEAKLDGLDF, string name, string MJMEBBCLHII)
	{
		if (MEEAKLDGLDF != null)
		{
			foreach (XmlNode childNode in MEEAKLDGLDF.ChildNodes)
			{
				if (childNode.Name.Equals(name))
				{
					XmlAttribute xmlAttribute = childNode.Attributes[MJMEBBCLHII];
					if (xmlAttribute != null)
					{
						return childNode;
					}
				}
			}
		}
		return null;
	}

	public static XmlNode FindChildWithAttribute(this XmlNode MEEAKLDGLDF, string name, string MJMEBBCLHII, string FOOKNBHPOOA)
	{
		if (MEEAKLDGLDF != null)
		{
			foreach (XmlNode childNode in MEEAKLDGLDF.ChildNodes)
			{
				if (childNode.Name.Equals(name))
				{
					XmlAttribute xmlAttribute = childNode.FindAttributeWithValue(MJMEBBCLHII, FOOKNBHPOOA);
					if (xmlAttribute != null)
					{
						return childNode;
					}
				}
			}
		}
		return null;
	}

	public static XmlAttribute FindAttributeWithValue(this XmlNode MEEAKLDGLDF, string MJMEBBCLHII, string FOOKNBHPOOA)
	{
		if (MEEAKLDGLDF != null)
		{
			foreach (XmlAttribute attribute in MEEAKLDGLDF.Attributes)
			{
				if (attribute.Name.Equals(MJMEBBCLHII) && attribute.Value.Equals(FOOKNBHPOOA))
				{
					return attribute;
				}
			}
		}
		return null;
	}

	public static XmlDocument LoadFromBytes(byte[] OIOHECBCFJA, bool LELJDDBPCNL = true)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = LELJDDBPCNL;
		try
		{
			MemoryStream stream = new MemoryStream(OIOHECBCFJA);
			using (XmlReader aEHOOKGCGLO = XmlReader.Create(stream, xmlReaderSettings))
			{
				return OpenXMLDocument(aEHOOKGCGLO);
			}
		}
		catch
		{
			return null;
		}
	}

	public static XmlDocument LoadFromString(string NGEPNAJJHCD, bool LELJDDBPCNL = true, bool PGKCOJBBOOH = false)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = LELJDDBPCNL;
		try
		{
			XmlDocument xmlDocument = ParseXmlText(NGEPNAJJHCD, xmlReaderSettings, PGKCOJBBOOH);
			if (xmlDocument == null)
			{
				Debug.LogError("Error open xml from string: " + NGEPNAJJHCD);
			}
			return xmlDocument;
		}
		catch
		{
			return null;
		}
	}

	public static XmlDocument OpenXMLDocument(string ONEIGMLOGDC, string LOBFDOKFJIP = "", XmlSourceMode HDCCAKLHKBD = XmlSourceMode.Normal, bool LELJDDBPCNL = true, bool PGKCOJBBOOH = false)
	{
		try
		{
			string text = ONEIGMLOGDC + ((!(LOBFDOKFJIP != string.Empty)) ? string.Empty : ("/" + LOBFDOKFJIP));
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = LELJDDBPCNL;
			// TexturePacker emits Apple plist files with a DOCTYPE declaration.
			// These files never need external entity resolution, but rejecting the
			// declaration makes every recovered location atlas look empty.
			xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
			xmlReaderSettings.XmlResolver = null;
			XmlDocument xmlDocument = null;
			switch (HDCCAKLHKBD)
			{
			case XmlSourceMode.Normal:
				xmlDocument = ((!ONEIGMLOGDC.StartsWith(SF2Paths.UserDataRoot)) ? ParseXmlText(ResourceManager.GetText(text), xmlReaderSettings, PGKCOJBBOOH) : ParseXmlText(ResourceManager.GetFileOrDevText(text), xmlReaderSettings, PGKCOJBBOOH));
				break;
			case XmlSourceMode.ForcedExternal:
				xmlDocument = ParseXmlText(ResourceManager.GetFileOrDevText(text), xmlReaderSettings, PGKCOJBBOOH);
				break;
			case XmlSourceMode.ForcedResourced:
				xmlDocument = ParseXmlText(ResourceManager.GetBundledOrModText(text), xmlReaderSettings, PGKCOJBBOOH);
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

	private static XmlDocument ParseXmlText(string GHDPPHAAPCA, XmlReaderSettings ENBBEFMEILD, bool PGKCOJBBOOH)
	{
		if (PGKCOJBBOOH && (string.IsNullOrEmpty(GHDPPHAAPCA) || !GHDPPHAAPCA.TrimStart().StartsWith("<")))
		{
			string text = XmlCryptoUtils.DecryptStringOrPassThrough(GHDPPHAAPCA);
			if (!string.IsNullOrEmpty(text))
			{
				GHDPPHAAPCA = text;
			}
		}
		using (XmlReader aEHOOKGCGLO = XmlReader.Create(new StringReader(GHDPPHAAPCA), ENBBEFMEILD))
		{
			return OpenXMLDocument(aEHOOKGCGLO);
		}
	}

	private static XmlDocument OpenXMLDocument(XmlReader AEHOOKGCGLO)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(AEHOOKGCGLO);
			return xmlDocument;
		}
		catch (Exception ex)
		{
			GameLog.Write(ex.ToString());
			return null;
		}
	}

	public static void TrimWhitespaceToFile(string AMNCLCPADOO, string IFIOLDFCLIE)
	{
		XmlDocument xmlDocument = OpenXMLDocument(AMNCLCPADOO, string.Empty, XmlSourceMode.ForcedExternal);
		if (xmlDocument == null)
		{
			Debug.LogError("[XmlUtils]: try to trim spaces from incorrect xml - " + AMNCLCPADOO);
			return;
		}
		try
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = false;
			xmlWriterSettings.NewLineChars = string.Empty;
			using (XmlWriter xmlWriter = XmlWriter.Create(IFIOLDFCLIE, xmlWriterSettings))
			{
				xmlDocument.Save(xmlWriter);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void TrimWhitespaceInPlace(string ONEIGMLOGDC)
	{
		TrimWhitespaceToFile(ONEIGMLOGDC, ONEIGMLOGDC);
	}

	public static string ToIndentedString(this XmlDocument GPIBAMAMGKD)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
		{
			GPIBAMAMGKD.Save(xmlWriter);
		}
		return stringBuilder.ToString();
	}

	public static XmlDocument LoadDocumentWithHashCheck(string ONEIGMLOGDC, string LOBFDOKFJIP = "", XmlSourceMode HDCCAKLHKBD = XmlSourceMode.Normal, bool LELJDDBPCNL = true)
	{
		if (HDCCAKLHKBD == XmlSourceMode.Normal || HDCCAKLHKBD == XmlSourceMode.ForcedExternal)
			Eclipse.Modding.ModRuntime.RecoverProfileSnapshot(string.IsNullOrEmpty(LOBFDOKFJIP) ? ONEIGMLOGDC : Path.Combine(ONEIGMLOGDC, LOBFDOKFJIP));
		XmlDocument xmlDocument = OpenXMLDocument(ONEIGMLOGDC, LOBFDOKFJIP, HDCCAKLHKBD, LELJDDBPCNL);
		if (xmlDocument != null && (HDCCAKLHKBD == XmlSourceMode.Normal || HDCCAKLHKBD == XmlSourceMode.ForcedExternal))
		{
			string oNEIGMLOGDC = ONEIGMLOGDC + ((!(LOBFDOKFJIP != string.Empty)) ? string.Empty : ("/" + LOBFDOKFJIP));
			UserDataValidator.CheckFileHash(xmlDocument, oNEIGMLOGDC);
		}
		return xmlDocument;
	}

	public static void SaveDocumentWithHash(XmlDocument JMCOLDENNDH, string KPFELJFPGHJ)
	{
		if (Eclipse.Modding.ModRuntime.TryWriteProfileSnapshot(JMCOLDENNDH, KPFELJFPGHJ)) return;
		JMCOLDENNDH.Save(KPFELJFPGHJ);
		UserDataValidator.UpdateFileHash(JMCOLDENNDH, KPFELJFPGHJ);
	}

	public static void CopyDocumentWithHash(string AMNCLCPADOO, string IFIOLDFCLIE)
	{
		XmlDocument xmlDocument = OpenXMLDocument(AMNCLCPADOO, string.Empty);
		xmlDocument.Save(IFIOLDFCLIE);
		UserDataValidator.UpdateFileHash(xmlDocument, IFIOLDFCLIE);
	}

	public static void UpdateHashForFile(string EFGLOMANJHN)
	{
		try
		{
			if (Path.GetExtension(EFGLOMANJHN).ToLower().Equals(".xml"))
			{
				XmlDocument lOBFDOKFJIP = OpenXMLDocument(EFGLOMANJHN, string.Empty);
				UserDataValidator.UpdateFileHash(lOBFDOKFJIP, EFGLOMANJHN);
			}
			else
			{
				UserDataValidator.UpdateFileHash(EFGLOMANJHN);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Exception: " + ex.Message);
		}
	}
}
