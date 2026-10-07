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
			DownloadPack downloadPack = ParsePack(item);
			AddBundle(downloadPack);
			_packs.Add(downloadPack);
		}
	}

	private void AddBundle(DownloadPack pack)
	{
		if (pack.Attach)
		{
			try
			{
				BundleManager.AddBundle(pack.Name);
			}
			catch (Exception ex)
			{
				GameLog.Error(ex.ToString());
			}
		}
	}

	private DownloadPack ParsePack(XmlNode node)
	{
		DownloadPack downloadPack = new DownloadPack();
		downloadPack.Name = node.Attributes["Name"].GetStringOrDefault();
		downloadPack.Url = node.Attributes["Url"].GetStringOrDefault();
		downloadPack.Version = node.Attributes["Version"].ParseInt();
		downloadPack.Size = node.Attributes["Size"].GetStringOrDefault();
		downloadPack.Reload = node.Attributes["Reload"].ParseBool();
		downloadPack.Attach = node.Attributes["Attach"].ParseBool();
		return downloadPack;
	}

	public bool IsPackByName(string name)
	{
		DownloadPack downloadPack = GeneralConfig.DownloadPacks.FindPack(name);
		foreach (DownloadPack item in _packs)
		{
			if (item.Name.Equals(name))
			{
				return downloadPack == null || downloadPack.Url.Equals(item.Url);
			}
		}
		return false;
	}

	public DownloadPack FindPack(string name)
	{
		return _packs.Find((DownloadPack entry) => entry.Name.Equals(name));
	}

	public void AddPack(string name, string url, string version, long endDate, bool attach)
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
		list.ForEach((XmlNode packNode) =>
		{
			packsXML.RemoveChild(packNode);
		});
		XmlNode xmlNode2 = packsXML.AppendElement("Pack");
		xmlNode2.AppendAttribute("Name").Value = name;
		xmlNode2.AppendAttribute("Url").Value = url;
		xmlNode2.AppendAttribute("Version").Value = version;
		xmlNode2.AppendAttribute("Attach").Value = ((!attach) ? "0" : "1");
		if (endDate >= 0)
		{
			xmlNode2.AppendAttribute("EndDate").Value = endDate.ToString();
		}
		_docPacks.Save(SF2Paths.GetPacksFilePath());
		DownloadPack downloadPack = _packs.Find((DownloadPack entry) => entry.Name.Equals(name));
		if (downloadPack != null)
		{
			_packs.Remove(downloadPack);
		}
		downloadPack = ParsePack(xmlNode2);
		if (downloadPack != null)
		{
			_packs.Add(downloadPack);
			AddBundle(downloadPack);
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
		DownloadPack downloadPack = _packs.Find((DownloadPack entry) => entry.Name.Equals(name));
		if (downloadPack != null)
		{
			_packs.Remove(downloadPack);
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
