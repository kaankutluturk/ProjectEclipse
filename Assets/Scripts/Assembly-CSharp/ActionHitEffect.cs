using System.Xml;

public class ActionHitEffect : ActionAnimation
{
	private string _FileName;

	public string FileName
	{
		get
		{
			return GetFileName();
		}
	}

	public ActionHitEffect(XmlNode node)
		: base(ActionType.HIT_EFFECT)
	{
		Parse(node);
	}

	public string GetFileName()
	{
		return _FileName;
	}

	public override void Visit(Model ACENLMONNPA)
	{
		ACENLMONNPA.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_FileName = node.Attributes["FileName"].GetStringOrDefault(string.Empty);
	}
}
