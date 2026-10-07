using System.Xml;

public class ActionTryOnEnd : ActionAnimation
{
	public ActionTryOnEnd(XmlNode node)
		: base(ActionType.DELETE)
	{
		Parse(node);
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
	}
}
