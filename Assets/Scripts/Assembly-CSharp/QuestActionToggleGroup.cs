using System.Xml;

public class QuestActionToggleGroup : QuestAction
{
	private string _toggle;

	private string _name;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_toggle = EPKLCPOEELO.Attributes["Toggle"].GetStringOrDefault("on");
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SetAbGroupToggle(_name, _toggle.Equals("on"));
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
	}
}
