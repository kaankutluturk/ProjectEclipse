using System.Xml;

public partial class PerkActionFlag : PerkActionModificator
{
	public PerkActionFlag()
	{
	}

	public PerkActionFlag(PerkActionFlag source)
		: base(source)
	{
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_FLAG);
	}
}
