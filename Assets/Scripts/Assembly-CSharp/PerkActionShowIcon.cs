using System.Diagnostics;
using System.Xml;

public class PerkActionShowIcon : PerkActionModificator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _image;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _showExpiration;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _expirationVer;

	public string ImageName
	{
		get
		{
			return GetImage();
		}
		protected set
		{
			set_Image(value);
		}
	}

	public bool ShowsExpiration
	{
		get
		{
			return GetShowExpiration();
		}
		protected set
		{
			set_ShowExpiration(value);
		}
	}

	public int ExpirationVersion
	{
		get
		{
			return GetExpirationVer();
		}
		protected set
		{
			set_ExpirationVer(value);
		}
	}

	public PerkActionShowIcon()
	{
	}

	public PerkActionShowIcon(PerkActionShowIcon NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_Image(NOLFMPDGCOC.GetImage());
		set_ShowExpiration(NOLFMPDGCOC.GetShowExpiration());
		set_ExpirationVer(NOLFMPDGCOC.GetExpirationVer());
	}

	public string GetImage()
	{
		return _image;
	}

	protected void set_Image(string value)
	{
		_image = value;
	}

	public bool GetShowExpiration()
	{
		return _showExpiration;
	}

	protected void set_ShowExpiration(bool value)
	{
		_showExpiration = value;
	}

	public int GetExpirationVer()
	{
		return _expirationVer;
	}

	protected void set_ExpirationVer(int value)
	{
		_expirationVer = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SHOW_ICONS);
		set_Image(node.Attributes["Image"].GetStringOrDefault(string.Empty));
		set_ShowExpiration(node.Attributes["ShowExpiration"].ParseBool());
		set_ExpirationVer(node.Attributes["ExpirationVer"].ParseInt());
	}
}
