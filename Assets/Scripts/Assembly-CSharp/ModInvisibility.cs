using System.Xml;

public class ModInvisibility : PerkActionModificator
{
	public ModInvisibility()
	{
	}

	public ModInvisibility(ModInvisibility other)
		: base(other)
	{
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_INVISIBILITY);
	}
}
