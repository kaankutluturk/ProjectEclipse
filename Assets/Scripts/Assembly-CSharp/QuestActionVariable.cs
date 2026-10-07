using System.Xml;

public class QuestActionVariable : QuestAction
{
	private string _name = string.Empty;

	private string _value = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_value = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ApplyVariable(parameters);
		FinishAction();
	}

	public void ApplyVariable(QuestParameters parameters)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		string variableValue = string.Empty;
		if (!string.IsNullOrEmpty(_value))
		{
			condition.SetValue(_value, result);
			variableValue = result.ToString();
		}
		result.Clear();
		condition.SetValue(_name, result);
		string variableName = result.ToString();
		Roster roster = ListSF.GetRoster();
		roster.SetQuestVariable(variableName, variableValue);
		ListSF.GetInstance().RequestSave();
	}
}
