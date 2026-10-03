using System.Collections.Generic;
using System.Xml;

public class ActionCreateModel : ActionAnimation
{
	public class ActionStruct
	{
		public string BOJPMNJDKJL;

		public string ModelName;

		public List<CopyItemInfo> OJIAKDDCGLB;

		public ActionStruct(string OLAAAIPEBBF, List<CopyItemInfo> _items, string _name = "")
		{
			BOJPMNJDKJL = OLAAAIPEBBF;
			ModelName = _name;
			OJIAKDDCGLB = _items;
		}
	}

	private string _Name;

	private string KLFLBKIIAOP;

	private string _StartAnimation;

	private List<CopyItemInfo> IOHGFGNNCFA = new List<CopyItemInfo>();

	public string ModelName
	{
		get
		{
			return AEGHBDJDPNA();
		}
	}

	public string BOJPMNJDKJL
	{
		get
		{
			return BNNBPLIJDGH();
		}
	}

	public List<CopyItemInfo> OJIAKDDCGLB
	{
		get
		{
			return DJBOFEEKJMP();
		}
	}

	public string EclipseProjectileOwner { get; private set; }
    public int EclipseProjectileLifetime { get; private set; }

	public string StartAnimation
	{
		get { return _StartAnimation; }
	}

	public ActionCreateModel(XmlNode node)
		: base(FADAJCEEKIO.CREATE_MODEL)
	{
		Parse(node);
	}

	public string AEGHBDJDPNA()
	{
		return _Name;
	}

	public string BNNBPLIJDGH()
	{
		return KLFLBKIIAOP;
	}

	public List<CopyItemInfo> DJBOFEEKJMP()
	{
		return IOHGFGNNCFA;
	}

	public override void Visit(Model ACENLMONNPA)
	{
		ACENLMONNPA.OPPIKLBKMPN(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
        EclipseProjectileOwner = ((XmlElement)node).GetAttribute("EclipseProjectileOwner");
        EclipseProjectileLifetime = Eclipse.Modding.ModProjectileLimits.DefaultLifetimeFrames;
        if (int.TryParse(((XmlElement)node).GetAttribute("EclipseProjectileLifetime"), out var lifetime))
            EclipseProjectileLifetime = lifetime;
		_Name = node.Attributes["Name"].CIPOICEEIBK(string.Empty);
		_StartAnimation = node.Attributes["StartAnimation"].CIPOICEEIBK(string.Empty);
		XmlElement xmlElement = node["Model"];
		KLFLBKIIAOP = ((xmlElement == null) ? null : xmlElement.Attributes["ItemType"].CIPOICEEIBK(string.Empty));
		foreach (XmlNode childNode in node.ChildNodes)
		{
			CopyItemInfo item = new CopyItemInfo(childNode, childNode.Attributes["CopyParentType"].CIPOICEEIBK(string.Empty), childNode.Attributes["CopyParentSubtype"].CIPOICEEIBK(string.Empty));
			IOHGFGNNCFA.Add(item);
		}
	}
}
