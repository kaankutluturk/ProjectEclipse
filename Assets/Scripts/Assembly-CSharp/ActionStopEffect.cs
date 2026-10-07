using System.Xml;

public class ActionStopEffect : ActionAnimation
{
	private string _Name;

	public ActionStopEffect(XmlNode node)
		: base(ActionType.STOP_EFFECT)
	{
		Parse(node);
	}

	public string get_Name()
	{
		return _Name;
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}
}
