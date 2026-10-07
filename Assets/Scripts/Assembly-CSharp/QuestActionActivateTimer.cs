using System.Xml;

public class QuestActionActivateTimer : QuestAction
{
	private string timerName;

	private string durationExpression;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		timerName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		durationExpression = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(durationExpression, lNIDLHOIHIM);
		long num = (long)lNIDLHOIHIM.resultNumber;
		long num2 = ListSF.GetCurrentTime();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		RosterTimerContainer kCMICMHCEBB = nKGLHEGIKKP.GetTimerContainer();
		kCMICMHCEBB.AddTimer(timerName, num2 + num);
		FinishAction();
	}
}
