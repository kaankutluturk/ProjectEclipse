using System.Xml;

public class CopyItemInfo : ItemInfo
{
	public string CopyParentType;

	public string CopyParentSubtype;

	public CopyItemInfo(XmlNode node, string FFBGOLDLBHD = "", string IGOAPEILEFP = "")
		: base(node)
	{
		CopyParentType = FFBGOLDLBHD;
		CopyParentSubtype = IGOAPEILEFP;
	}
}
