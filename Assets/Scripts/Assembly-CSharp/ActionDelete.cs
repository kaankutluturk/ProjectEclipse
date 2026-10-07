using System.Xml;

public class ActionDelete : ActionAnimation
{
	public ActionDelete(XmlNode node)
		: base(ActionType.DELETE)
	{
		Parse(node);
	}

	public override void Visit(Model ACENLMONNPA)
	{
		ACENLMONNPA.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
	}
}
