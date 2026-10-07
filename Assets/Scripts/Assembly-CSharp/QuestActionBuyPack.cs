using System.Xml;

public class QuestActionBuyPack : QuestAction
{
	private string _PackName;

	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_PackName = node.Attributes["PackName"].GetStringOrDefault(string.Empty);
		XmlNode successNode = node["Success"];
		XmlNode sequenceNode = node["Error"];
		ParseSequenceWithUnlock(successNode, successSequence, OnActionComplete);
		ParseSequenceWithUnlock(sequenceNode, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_PackName, result);
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
