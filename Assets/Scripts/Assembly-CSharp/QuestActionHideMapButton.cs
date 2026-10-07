using System.Xml;

public class QuestActionHideMapButton : QuestAction
{
	private string _name = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		string name = string.Empty;
		GetValues(ref name);
		MapButtonController.GetInstance().RemoveButton(name);
		FinishAction();
	}

	private void GetValues(ref string name)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(_name, result);
		name = result.ToString();
	}
}
