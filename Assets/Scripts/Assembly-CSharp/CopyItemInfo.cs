using System.Xml;

public class CopyItemInfo : ItemInfo
{
	public string CopyParentType;

	public string CopyParentSubtype;

	public CopyItemInfo(XmlNode node, string parentType = "", string parentSubtype = "")
		: base(node)
	{
		CopyParentType = parentType;
		CopyParentSubtype = parentSubtype;
	}
}
