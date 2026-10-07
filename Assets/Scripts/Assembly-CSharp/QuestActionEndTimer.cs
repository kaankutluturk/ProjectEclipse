using System.Xml;

public class QuestActionEndTimer : QuestAction
{
	private string timerName;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		timerName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		RosterTimerContainer kCMICMHCEBB = nKGLHEGIKKP.GetTimerContainer();
		kCMICMHCEBB.RemoveTimer(timerName);
		FinishAction();
	}
}
