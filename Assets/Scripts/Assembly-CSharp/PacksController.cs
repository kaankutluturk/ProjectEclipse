using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class PacksController
{
	private static PacksController _Instance;

	private List<DownloadPack> _packs = new List<DownloadPack>();

	private XmlDocument _docPacks;

	public static PacksController Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public List<DownloadPack> PackList
	{
		get
		{
			return GetPacks();
		}
	}

	public static PacksController GetInstance()
	{
		if (_Instance == null)
		{
			_Instance = new PacksController();
		}
		return _Instance;
	}

	public List<DownloadPack> GetPacks()
	{
		return _packs;
	}

	public void LoadPacks()
	{
		_docPacks = XmlUtils.OpenXMLDocument(SF2Paths.GetPacksFilePath(), string.Empty, XmlUtils.XmlSourceMode.ForcedExternal);
		if (_docPacks == null)
		{
			_docPacks = new XmlDocument();
		}
		XmlNode xmlNode = _docPacks["Packs"];
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNode)
		{
			DownloadPack jBKAOMLJCEL = ParsePack(item);
			AddBundle(jBKAOMLJCEL);
			_packs.Add(jBKAOMLJCEL);
		}
	}

	private void AddBundle(DownloadPack LMHLLOBNKMB)
	{
		if (LMHLLOBNKMB.Attach)
		{
			try
			{
				BundleManager.AddBundle(LMHLLOBNKMB.Name);
			}
			catch (Exception ex)
			{
				GameLog.Error(ex.ToString());
			}
		}
	}

	private DownloadPack ParsePack(XmlNode node)
	{
		DownloadPack jBKAOMLJCEL = new DownloadPack();
		jBKAOMLJCEL.Name = node.Attributes["Name"].GetStringOrDefault();
		jBKAOMLJCEL.Url = node.Attributes["Url"].GetStringOrDefault();
		jBKAOMLJCEL.Version = node.Attributes["Version"].ParseInt();
		jBKAOMLJCEL.Size = node.Attributes["Size"].GetStringOrDefault();
		jBKAOMLJCEL.Reload = node.Attributes["Reload"].ParseBool();
		jBKAOMLJCEL.Attach = node.Attributes["Attach"].ParseBool();
		return jBKAOMLJCEL;
	}

	public bool IsPackByName(string name)
	{
		DownloadPack jBKAOMLJCEL = GeneralConfig.DownloadPacks.FindPack(name);
		foreach (DownloadPack item in _packs)
		{
			if (item.Name.Equals(name))
			{
				return jBKAOMLJCEL == null || jBKAOMLJCEL.Url.Equals(item.Url);
			}
		}
		return false;
	}

	public DownloadPack FindPack(string name)
	{
		return _packs.Find((DownloadPack DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
	}

	public void AddPack(string name, string BEPKJNKCKPH, string version, long NKKKMPPEMKE, bool AHDLCJFCJMJ)
	{
		if (_docPacks == null)
		{
			GameLog.Error("PacksContainer.AddPack _docPacks is null");
			return;
		}
		XmlNode packsXML = _docPacks["Packs"];
		if (packsXML == null)
		{
			packsXML = _docPacks.AppendNewNode("Packs");
		}
		List<XmlNode> list = new List<XmlNode>();
		foreach (XmlNode childNode in packsXML.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault();
			if (text.Equals(name))
			{
				list.Add(childNode);
			}
		}
		list.ForEach((XmlNode DHDMNHCIPEH) =>
		{
			packsXML.RemoveChild(DHDMNHCIPEH);
		});
		XmlNode xmlNode2 = packsXML.AppendElement("Pack");
		xmlNode2.AppendAttribute("Name").Value = name;
		xmlNode2.AppendAttribute("Url").Value = BEPKJNKCKPH;
		xmlNode2.AppendAttribute("Version").Value = version;
		xmlNode2.AppendAttribute("Attach").Value = ((!AHDLCJFCJMJ) ? "0" : "1");
		if (NKKKMPPEMKE >= 0)
		{
			xmlNode2.AppendAttribute("EndDate").Value = NKKKMPPEMKE.ToString();
		}
		_docPacks.Save(SF2Paths.GetPacksFilePath());
		DownloadPack jBKAOMLJCEL = _packs.Find((DownloadPack DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
		if (jBKAOMLJCEL != null)
		{
			_packs.Remove(jBKAOMLJCEL);
		}
		jBKAOMLJCEL = ParsePack(xmlNode2);
		if (jBKAOMLJCEL != null)
		{
			_packs.Add(jBKAOMLJCEL);
			AddBundle(jBKAOMLJCEL);
		}
	}

	public void DeletePack(string name)
	{
		if (_docPacks == null)
		{
			GameLog.Error("PacksContainer.AddPack _docPacks is null");
			return;
		}
		XmlNode xmlNode = _docPacks["Packs"];
		if (xmlNode == null)
		{
			xmlNode = _docPacks.CreateNode(XmlNodeType.Element, "Packs", null);
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault();
			if (text.Equals(name))
			{
				xmlNode.RemoveChild(childNode);
			}
		}
		_docPacks.Save(SF2Paths.GetPacksFilePath());
		DownloadPack jBKAOMLJCEL = _packs.Find((DownloadPack DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
		if (jBKAOMLJCEL != null)
		{
			_packs.Remove(jBKAOMLJCEL);
		}
	}

	public void Reset()
	{
		_packs.Clear();
		BundleManager.Reset();
	}

	public void DeletePacksFile()
	{
		if (File.Exists(SF2Paths.GetPacksFilePath()))
		{
			File.Delete(SF2Paths.GetPacksFilePath());
			UserDataValidator.DeleteHashFile(SF2Paths.GetPacksFilePath());
		}
		SF2Paths.ResetBundlesDirectory();
	}
}
