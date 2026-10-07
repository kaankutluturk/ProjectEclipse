using System.Xml;

public class QuestActionBuyPack : QuestAction
{
	private string _PackName;

	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_PackName = EPKLCPOEELO.Attributes["PackName"].GetStringOrDefault(string.Empty);
		XmlNode ePKLCPOEELO = EPKLCPOEELO["Success"];
		XmlNode ePKLCPOEELO2 = EPKLCPOEELO["Error"];
		ParseSequenceWithUnlock(ePKLCPOEELO, successSequence, OnActionComplete);
		ParseSequenceWithUnlock(ePKLCPOEELO2, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		ResetSequences();
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult bMDEBHIHIAJ = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(_PackName, bMDEBHIHIAJ);
	}

	public override void ResetSequences()
	{
		successSequence.Reset();
		errorSequence.Reset();
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}
}
