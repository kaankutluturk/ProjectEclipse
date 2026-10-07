using System.Collections.Generic;
using System.Xml;

public class ActionCreateModel : ActionAnimation
{
	public class ActionStruct
	{
		public string ItemType;

		public string ModelName;

		public List<CopyItemInfo> CopyItems;

		public ActionStruct(string OLAAAIPEBBF, List<CopyItemInfo> _items, string _name = "")
		{
			ItemType = OLAAAIPEBBF;
			ModelName = _name;
			CopyItems = _items;
		}
	}

	private string _Name;

	private string _ItemType;

	private string _StartAnimation;

	private List<CopyItemInfo> _CopyItems = new List<CopyItemInfo>();

	public string ModelName
	{
		get
		{
			return GetModelName();
		}
	}

	public string ItemType
	{
		get
		{
			return GetItemType();
		}
	}

	public List<CopyItemInfo> CopyItems
	{
		get
		{
			return GetCopyItems();
		}
	}

	public string EclipseProjectileOwner { get; private set; }
    public int EclipseProjectileLifetime { get; private set; }

	public string StartAnimation
	{
		get { return _StartAnimation; }
	}

	public ActionCreateModel(XmlNode node)
		: base(ActionType.CREATE_MODEL)
	{
		Parse(node);
	}

	public string GetModelName()
	{
		return _Name;
	}

	public string GetItemType()
	{
		return _ItemType;
	}

	public List<CopyItemInfo> GetCopyItems()
	{
		return _CopyItems;
	}

	public override void Visit(Model ACENLMONNPA)
	{
		ACENLMONNPA.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
        EclipseProjectileOwner = ((XmlElement)node).GetAttribute("EclipseProjectileOwner");
        EclipseProjectileLifetime = Eclipse.Modding.ModProjectileLimits.DefaultLifetimeFrames;
        if (int.TryParse(((XmlElement)node).GetAttribute("EclipseProjectileLifetime"), out var lifetime))
            EclipseProjectileLifetime = lifetime;
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_StartAnimation = node.Attributes["StartAnimation"].GetStringOrDefault(string.Empty);
		XmlElement xmlElement = node["Model"];
		_ItemType = ((xmlElement == null) ? null : xmlElement.Attributes["ItemType"].GetStringOrDefault(string.Empty));
		foreach (XmlNode childNode in node.ChildNodes)
		{
			CopyItemInfo item = new CopyItemInfo(childNode, childNode.Attributes["CopyParentType"].GetStringOrDefault(string.Empty), childNode.Attributes["CopyParentSubtype"].GetStringOrDefault(string.Empty));
			_CopyItems.Add(item);
		}
	}
}
